using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UnitySkills
{
    /// <summary>
    /// Script management skills - create, read, modify.
    /// </summary>
    public static class ScriptSkills
    {
        private const int DefaultDiagnosticLimit = 20;

        [UnitySkill("script_create", "Create a new C# script. Optional: namespace")]
        public static object ScriptCreate(
            string scriptName = null,
            string name = null,
            string folder = "Assets/Scripts",
            string template = null,
            string namespaceName = null,
            bool checkCompile = true,
            int diagnosticLimit = DefaultDiagnosticLimit)
        {
            scriptName = scriptName ?? name;
            if (string.IsNullOrEmpty(scriptName))
                return new { error = "scriptName is required" };
            if (HasPathSeparators(scriptName))
                return new { error = "scriptName must not contain path separators" };

            if (!string.IsNullOrEmpty(folder) && Validate.SafePath(folder, "folder") is object folderErr) return folderErr;

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var path = Path.Combine(folder, scriptName + ".cs");
            if (File.Exists(path))
                return new { error = $"Script already exists: {path}" };

            string content = template;
            if (string.IsNullOrEmpty(content))
            {
                content = @"using UnityEngine;

namespace {NAMESPACE}
{
    public class {CLASS} : MonoBehaviour
    {
        void Start()
        {
        }

        void Update()
        {
        }
    }
}
";

                if (string.IsNullOrEmpty(namespaceName))
                {
                    content = @"using UnityEngine;

public class {CLASS} : MonoBehaviour
{
    void Start()
    {
    }

    void Update()
    {
    }
}
";
                }
            }

            content = content.Replace("{CLASS}", scriptName);
            content = content.Replace("{NAMESPACE}", string.IsNullOrEmpty(namespaceName) ? "DefaultNamespace" : namespaceName);

            File.WriteAllText(path, content);
            AssetDatabase.ImportAsset(path);

            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (asset != null) WorkflowManager.SnapshotObject(asset, SnapshotType.Created);

            var result = CreateScriptMutationResult(path, checkCompile, diagnosticLimit);
            result["className"] = scriptName;
            result["namespaceName"] = namespaceName;
            return result;
        }

        [UnitySkill("script_create_batch", "Create multiple scripts (Efficient). items: JSON array of {scriptName, folder, template, namespace}")]
        public static object ScriptCreateBatch(string items)
        {
            return BatchExecutor.Execute<BatchScriptItem>(items, item =>
            {
                var result = ScriptCreate(item.scriptName ?? item.name, null, item.folder ?? "Assets/Scripts", item.template, item.namespaceName);
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(result);
                if (json.Contains("\"error\""))
                    throw new System.Exception(((dynamic)result).error);
                return result;
            }, item => item.scriptName ?? item.name);
        }

        private class BatchScriptItem
        {
            public string scriptName { get; set; }
            public string name { get; set; }
            public string folder { get; set; }
            public string template { get; set; }
            public string namespaceName { get; set; }
        }

        [UnitySkill("script_read", "Read the contents of a script")]
        public static object ScriptRead(string scriptPath)
        {
            if (Validate.SafePath(scriptPath, "scriptPath") is object pathErr) return pathErr;
            if (!File.Exists(scriptPath))
                return new { error = $"Script not found: {scriptPath}" };

            var content = File.ReadAllText(scriptPath);
            return new { path = NormalizePath(scriptPath), lines = content.Split('\n').Length, content };
        }

        [UnitySkill("script_delete", "Delete a script file")]
        public static object ScriptDelete(string scriptPath)
        {
            if (!File.Exists(scriptPath))
                return new { error = $"Script not found: {scriptPath}" };
            if (Validate.SafePath(scriptPath, "scriptPath", isDelete: true) is object pathErr) return pathErr;

            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(scriptPath);
            if (asset != null) WorkflowManager.SnapshotObject(asset);

            AssetDatabase.DeleteAsset(scriptPath);
            var result = new Dictionary<string, object>
            {
                ["success"] = true,
                ["deleted"] = NormalizePath(scriptPath)
            };
            ServerAvailabilityHelper.AttachTransientUnavailableNotice(
                result,
                $"脚本资源已删除: {NormalizePath(scriptPath)}。Unity 可能短暂重载脚本域。",
                alwaysInclude: true);
            return result;
        }

        [UnitySkill("script_find_in_file", "Search for pattern in scripts")]
        public static object ScriptFindInFile(string pattern, string folder = "Assets", bool isRegex = false, int limit = 50)
        {
            if (!string.IsNullOrEmpty(folder) && Validate.SafePath(folder, "folder") is object folderErr) return folderErr;

            var results = new List<object>();
            var files = Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if (results.Count >= limit) break;

                var lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    bool match = isRegex
                        ? Regex.IsMatch(lines[i], pattern, RegexOptions.None, System.TimeSpan.FromSeconds(1))
                        : lines[i].Contains(pattern);

                    if (!match) continue;

                    results.Add(new
                    {
                        file = NormalizePath(file),
                        line = i + 1,
                        content = lines[i].Trim()
                    });

                    if (results.Count >= limit) break;
                }
            }

            return new { pattern, matchCount = results.Count, matches = results };
        }

        [UnitySkill("script_append", "Append content to a script")]
        public static object ScriptAppend(string scriptPath, string content, int atLine = -1, bool checkCompile = true, int diagnosticLimit = DefaultDiagnosticLimit)
        {
            if (!File.Exists(scriptPath))
                return new { error = $"Script not found: {scriptPath}" };
            if (Validate.SafePath(scriptPath, "scriptPath") is object pathErr) return pathErr;

            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(scriptPath);
            if (asset != null) WorkflowManager.SnapshotObject(asset);

            var lines = File.ReadAllLines(scriptPath).ToList();
            if (atLine < 0 || atLine >= lines.Count)
            {
                var lastBrace = lines.FindLastIndex(l => l.Trim() == "}");
                if (lastBrace > 0) lines.Insert(lastBrace, content);
                else lines.Add(content);
            }
            else
            {
                lines.Insert(atLine, content);
            }

            File.WriteAllLines(scriptPath, lines);
            AssetDatabase.ImportAsset(scriptPath);
            return CreateScriptMutationResult(scriptPath, checkCompile, diagnosticLimit);
        }

        [UnitySkill("script_replace", "Find and replace content in a script file")]
        public static object ScriptReplace(string scriptPath, string find, string replace, bool isRegex = false, bool checkCompile = true, int diagnosticLimit = DefaultDiagnosticLimit)
        {
            if (!File.Exists(scriptPath))
                return new { error = $"Script not found: {scriptPath}" };
            if (Validate.SafePath(scriptPath, "scriptPath") is object pathErr) return pathErr;

            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(scriptPath);
            if (asset != null) WorkflowManager.SnapshotObject(asset);

            var content = File.ReadAllText(scriptPath);
            string newContent = isRegex
                ? Regex.Replace(content, find, replace, RegexOptions.None, System.TimeSpan.FromSeconds(2))
                : content.Replace(find, replace);
            int changes = isRegex
                ? Regex.Matches(content, find, RegexOptions.None, System.TimeSpan.FromSeconds(2)).Count
                : (content.Length - content.Replace(find, "").Length) / (find.Length > 0 ? find.Length : 1);

            File.WriteAllText(scriptPath, newContent);
            AssetDatabase.ImportAsset(scriptPath);

            var result = CreateScriptMutationResult(scriptPath, checkCompile, diagnosticLimit);
            result["replacements"] = changes;
            return result;
        }

        [UnitySkill("script_list", "List C# script files in the project")]
        public static object ScriptList(string folder = "Assets", string filter = null, int limit = 100)
        {
            var guids = AssetDatabase.FindAssets("t:MonoScript", new[] { folder });
            var scripts = guids
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Where(p => p.EndsWith(".cs"))
                .Where(p => string.IsNullOrEmpty(filter) || p.Contains(filter))
                .Take(limit)
                .Select(p => new { path = p, name = Path.GetFileNameWithoutExtension(p) })
                .ToArray();

            return new { count = scripts.Length, scripts };
        }

        [UnitySkill("script_get_info", "Get script info (class name, base class, methods)")]
        public static object ScriptGetInfo(string scriptPath)
        {
            var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            if (monoScript == null) return new { error = $"MonoScript not found: {scriptPath}" };

            var type = monoScript.GetClass();
            if (type == null) return new { path = NormalizePath(scriptPath), className = "(unknown)", note = "Class not yet compiled or abstract" };

            var methods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)
                .Select(m => m.Name)
                .ToArray();
            var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Select(f => new { name = f.Name, type = f.FieldType.Name })
                .ToArray();

            return new
            {
                path = NormalizePath(scriptPath),
                className = type.Name,
                baseClass = type.BaseType?.Name,
                namespaceName = type.Namespace,
                isMonoBehaviour = typeof(MonoBehaviour).IsAssignableFrom(type),
                publicMethods = methods,
                publicFields = fields
            };
        }

        [UnitySkill("script_rename", "Rename a script file")]
        public static object ScriptRename(string scriptPath, string newName, bool checkCompile = true, int diagnosticLimit = DefaultDiagnosticLimit)
        {
            if (!File.Exists(scriptPath)) return new { error = $"Script not found: {scriptPath}" };
            if (Validate.SafePath(scriptPath, "scriptPath") is object pathErr) return pathErr;
            if (Validate.Required(newName, "newName") is object newNameErr) return newNameErr;
            if (HasPathSeparators(newName))
                return new { error = "newName must not contain path separators" };

            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(scriptPath);
            if (asset != null) WorkflowManager.SnapshotObject(asset);

            var renameResult = AssetDatabase.RenameAsset(scriptPath, newName);
            if (!string.IsNullOrEmpty(renameResult)) return new { error = renameResult };

            var renamedPath = Path.Combine(Path.GetDirectoryName(scriptPath) ?? "", newName + ".cs");
            var result = CreateScriptMutationResult(renamedPath, checkCompile, diagnosticLimit);
            result["oldPath"] = NormalizePath(scriptPath);
            result["newName"] = newName;
            return result;
        }

        [UnitySkill("script_move", "Move a script to a new folder")]
        public static object ScriptMove(string scriptPath, string newFolder, bool checkCompile = true, int diagnosticLimit = DefaultDiagnosticLimit)
        {
            if (!File.Exists(scriptPath)) return new { error = $"Script not found: {scriptPath}" };
            if (Validate.SafePath(scriptPath, "scriptPath") is object pathErr) return pathErr;
            if (Validate.SafePath(newFolder, "newFolder") is object folderErr) return folderErr;

            if (!Directory.Exists(newFolder)) Directory.CreateDirectory(newFolder);

            var fileName = Path.GetFileName(scriptPath);
            var newPath = Path.Combine(newFolder, fileName);
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(scriptPath);
            if (asset != null) WorkflowManager.SnapshotObject(asset);

            var moveResult = AssetDatabase.MoveAsset(scriptPath, newPath);
            if (!string.IsNullOrEmpty(moveResult)) return new { error = moveResult };

            var result = CreateScriptMutationResult(newPath, checkCompile, diagnosticLimit);
            result["oldPath"] = NormalizePath(scriptPath);
            result["newPath"] = NormalizePath(newPath);
            return result;
        }

        [UnitySkill("script_get_compile_feedback", "Get compile diagnostics related to a specific script. Use after script_create/script_append/script_replace/script_rename/script_move.")]
        public static object ScriptGetCompileFeedback(string scriptPath, int limit = DefaultDiagnosticLimit)
        {
            if (Validate.SafePath(scriptPath, "scriptPath") is object pathErr) return pathErr;
            if (!File.Exists(scriptPath)) return new { error = $"Script not found: {scriptPath}" };
            return GetCompilationFeedback(scriptPath, limit);
        }

        private static Dictionary<string, object> CreateScriptMutationResult(string scriptPath, bool checkCompile, int diagnosticLimit)
        {
            var result = new Dictionary<string, object>
            {
                ["success"] = true,
                ["path"] = NormalizePath(scriptPath)
            };

            if (checkCompile)
                result["compilation"] = GetCompilationFeedback(scriptPath, diagnosticLimit);

            ServerAvailabilityHelper.AttachTransientUnavailableNotice(
                result,
                $"脚本资源已变更: {NormalizePath(scriptPath)}。Unity 可能短暂重载脚本域。",
                alwaysInclude: true);

            return result;
        }

        private static Dictionary<string, object> GetCompilationFeedback(string scriptPath, int limit)
        {
            string normalizedPath = NormalizePath(scriptPath);
            string fileName = Path.GetFileName(normalizedPath);
            string className = Path.GetFileNameWithoutExtension(normalizedPath);
            bool isCompiling = EditorApplication.isCompiling || EditorApplication.isUpdating;

            var diagnostics = FindRelevantCompileErrors(normalizedPath, fileName, className, limit)
                .Select(log => new
                {
                    type = log.type,
                    message = log.message,
                    file = NormalizePath(log.file),
                    line = log.line
                })
                .ToArray();

            return new Dictionary<string, object>
            {
                ["scriptPath"] = normalizedPath,
                ["isCompiling"] = isCompiling,
                ["hasErrors"] = diagnostics.Length > 0,
                ["errorCount"] = diagnostics.Length,
                ["errors"] = diagnostics,
                ["nextAction"] = isCompiling
                    ? "Unity is still compiling. Call script_get_compile_feedback again after compilation finishes."
                    : diagnostics.Length > 0
                        ? "Fix the script based on the reported errors, then call script_get_compile_feedback again."
                        : "No compile errors were found for this script."
            };
        }

        private static IEnumerable<DebugSkills.LogEntryInfo> FindRelevantCompileErrors(string normalizedPath, string fileName, string className, int limit)
        {
            int searchLimit = Mathf.Max(Mathf.Max(limit, DefaultDiagnosticLimit), 1) * 5;
            var logs = DebugSkills.ReadLogEntries(DebugSkills.ErrorModeMask, null, searchLimit);
            return logs
                .Where(log => IsRelevantCompileError(log, normalizedPath, fileName, className))
                .Take(Mathf.Max(limit, 1));
        }

        private static bool IsRelevantCompileError(DebugSkills.LogEntryInfo log, string normalizedPath, string fileName, string className)
        {
            if (log == null) return false;

            string logFile = NormalizePath(log.file);
            if (!string.IsNullOrEmpty(logFile))
            {
                if (logFile.EndsWith(normalizedPath, System.StringComparison.OrdinalIgnoreCase) ||
                    logFile.EndsWith("/" + fileName, System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(Path.GetFileName(logFile), fileName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            string message = log.message ?? "";
            return message.IndexOf(fileName, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                   (!string.IsNullOrEmpty(className) && message.IndexOf(className, System.StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static bool HasPathSeparators(string value)
        {
            return value.Contains("/") || value.Contains("\\") || value.Contains("..");
        }

        private static string NormalizePath(string path)
        {
            return string.IsNullOrEmpty(path) ? path : path.Replace("\\", "/");
        }
    }
}
