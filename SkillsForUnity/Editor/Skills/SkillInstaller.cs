using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace UnitySkills
{
    /// <summary>
    /// One-click skill installer for mainstream AI IDEs: Claude Code, Antigravity, Gemini CLI, Codex, and Cursor.
    /// </summary>
    public static class SkillInstaller
    {
        // Claude Code paths - Claude supports any folder name
        public static string ClaudeProjectPath => Path.Combine(Application.dataPath, "..", ".claude", "skills", "unity-skills");
        public static string ClaudeGlobalPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude", "skills", "unity-skills");
        
        // Antigravity paths
        public static string AntigravityProjectPath => Path.Combine(Application.dataPath, "..", ".agent", "skills", "unity-skills");
        public static string AntigravityGlobalPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gemini", "antigravity", "skills", "unity-skills");
        public static string AntigravityWorkflowProjectPath => Path.Combine(Application.dataPath, "..", ".agent", "workflows");
        public static string AntigravityWorkflowGlobalPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gemini", "antigravity", "workflows");

        // Gemini CLI paths - folder name should match SKILL.md name field for proper recognition
        public static string GeminiProjectPath => Path.Combine(Application.dataPath, "..", ".gemini", "skills", "unity-skills");
        public static string GeminiGlobalPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gemini", "skills", "unity-skills");

        // Codex paths - https://developers.openai.com/codex/skills
        public static string CodexProjectPath => Path.Combine(Application.dataPath, "..", ".codex", "skills", "unity-skills");
        public static string CodexGlobalPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".codex", "skills", "unity-skills");

        // Cursor paths - https://cursor.com/docs/context/skills
        public static string CursorProjectPath => Path.Combine(Application.dataPath, "..", ".cursor", "skills", "unity-skills");
        public static string CursorGlobalPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cursor", "skills", "unity-skills");

        public static bool IsClaudeProjectInstalled => Directory.Exists(ClaudeProjectPath) && File.Exists(Path.Combine(ClaudeProjectPath, "SKILL.md"));
        public static bool IsClaudeGlobalInstalled => Directory.Exists(ClaudeGlobalPath) && File.Exists(Path.Combine(ClaudeGlobalPath, "SKILL.md"));
        public static bool IsAntigravityProjectInstalled => Directory.Exists(AntigravityProjectPath) && File.Exists(Path.Combine(AntigravityProjectPath, "SKILL.md"));
        public static bool IsAntigravityGlobalInstalled => Directory.Exists(AntigravityGlobalPath) && File.Exists(Path.Combine(AntigravityGlobalPath, "SKILL.md"));
        public static bool IsGeminiProjectInstalled => Directory.Exists(GeminiProjectPath) && File.Exists(Path.Combine(GeminiProjectPath, "SKILL.md"));
        public static bool IsGeminiGlobalInstalled => Directory.Exists(GeminiGlobalPath) && File.Exists(Path.Combine(GeminiGlobalPath, "SKILL.md"));
        public static bool IsCodexProjectInstalled => Directory.Exists(CodexProjectPath) && File.Exists(Path.Combine(CodexProjectPath, "SKILL.md"));
        public static bool IsCodexGlobalInstalled => Directory.Exists(CodexGlobalPath) && File.Exists(Path.Combine(CodexGlobalPath, "SKILL.md"));
        public static bool IsCursorProjectInstalled => Directory.Exists(CursorProjectPath) && File.Exists(Path.Combine(CursorProjectPath, "SKILL.md"));
        public static bool IsCursorGlobalInstalled => Directory.Exists(CursorGlobalPath) && File.Exists(Path.Combine(CursorGlobalPath, "SKILL.md"));

        public static (bool success, string message) InstallClaude(bool global)
        {
            try
            {
                var targetPath = global ? ClaudeGlobalPath : ClaudeProjectPath;
                return InstallSkill(targetPath, "Claude Code", "ClaudeCode");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static (bool success, string message) InstallAntigravity(bool global)
        {
            try
            {
                var targetPath = global ? AntigravityGlobalPath : AntigravityProjectPath;
                var res = InstallSkill(targetPath, "Antigravity", "Antigravity");
                if (!res.success) return res;

                // Install Workflow for Antigravity slash commands
                var workflowPath = global ? AntigravityWorkflowGlobalPath : AntigravityWorkflowProjectPath;
                if (!Directory.Exists(workflowPath))
                    Directory.CreateDirectory(workflowPath);
                
                var workflowMd = GenerateAntigravityWorkflow();
                var utf8NoBom = new UTF8Encoding(false);
                File.WriteAllText(Path.Combine(workflowPath, "unity-skills.md"), workflowMd.Replace("\r\n", "\n"), utf8NoBom);
                
                return (true, targetPath);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static (bool success, string message) UninstallClaude(bool global)
        {
            try
            {
                var targetPath = global ? ClaudeGlobalPath : ClaudeProjectPath;
                return UninstallSkill(targetPath, "Claude Code");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static (bool success, string message) UninstallAntigravity(bool global)
        {
            try
            {
                var targetPath = global ? AntigravityGlobalPath : AntigravityProjectPath;
                var res = UninstallSkill(targetPath, "Antigravity");

                // Uninstall Workflow
                var workflowPath = global ? AntigravityWorkflowGlobalPath : AntigravityWorkflowProjectPath;
                var workflowFile = Path.Combine(workflowPath, "unity-skills.md");
                if (File.Exists(workflowFile))
                    File.Delete(workflowFile);

                return res;
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static (bool success, string message) InstallGemini(bool global)
        {
            try
            {
                var targetPath = global ? GeminiGlobalPath : GeminiProjectPath;
                return InstallSkill(targetPath, "Gemini CLI", "Gemini");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static (bool success, string message) UninstallGemini(bool global)
        {
            try
            {
                var targetPath = global ? GeminiGlobalPath : GeminiProjectPath;
                return UninstallSkill(targetPath, "Gemini CLI");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static (bool success, string message) InstallCodex(bool global)
        {
            try
            {
                var targetPath = global ? CodexGlobalPath : CodexProjectPath;
                var res = InstallSkill(targetPath, "Codex", "Codex");
                if (!res.success) return res;

                // For project-level install, also update AGENTS.md
                if (!global)
                {
                    UpdateAgentsMd();
                }
                
                return res;
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static (bool success, string message) UninstallCodex(bool global)
        {
            try
            {
                var targetPath = global ? CodexGlobalPath : CodexProjectPath;
                var res = UninstallSkill(targetPath, "Codex");

                // For project-level uninstall, also remove from AGENTS.md
                if (!global)
                {
                    RemoveFromAgentsMd();
                }

                return res;
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static (bool success, string message) InstallCursor(bool global)
        {
            try
            {
                var targetPath = global ? CursorGlobalPath : CursorProjectPath;
                return InstallSkill(targetPath, "Cursor", "Cursor");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static (bool success, string message) UninstallCursor(bool global)
        {
            try
            {
                var targetPath = global ? CursorGlobalPath : CursorProjectPath;
                return UninstallSkill(targetPath, "Cursor");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static (bool success, string message) InstallCustom(string path, string agentName = "Custom")
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return (false, "Path cannot be empty");

                return InstallSkill(path, "Custom Path", agentName);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private static string AgentsMdPath => Path.Combine(Application.dataPath, "..", "AGENTS.md");
        private const string UnitySkillsEntry = "- unity-skills: Unity Editor automation via REST API";

        private static void UpdateAgentsMd()
        {
            var agentsPath = AgentsMdPath;
            var utf8NoBom = new UTF8Encoding(false);

            if (File.Exists(agentsPath))
            {
                // File exists, check if unity-skills is already declared
                var content = File.ReadAllText(agentsPath);
                if (!content.Contains("unity-skills"))
                {
                    // Append unity-skills entry
                    var appendContent = "\n\n## UnitySkills\n" + UnitySkillsEntry + "\n";
                    File.AppendAllText(agentsPath, appendContent.Replace("\r\n", "\n"), utf8NoBom);
                    SkillsLogger.Log("Added unity-skills to existing AGENTS.md");
                }
                else
                {
                    SkillsLogger.LogVerbose("unity-skills already declared in AGENTS.md");
                }
            }
            else
            {
                // Create new AGENTS.md
                var newContent = @"# AGENTS.md

This file declares available skills for AI agents like Codex.

## UnitySkills
" + UnitySkillsEntry + "\n";
                File.WriteAllText(agentsPath, newContent.Replace("\r\n", "\n"), utf8NoBom);
                SkillsLogger.Log("Created AGENTS.md with unity-skills declaration");
            }
        }

        private static void RemoveFromAgentsMd()
        {
            var agentsPath = AgentsMdPath;
            if (!File.Exists(agentsPath)) return;

            var content = File.ReadAllText(agentsPath);
            if (content.Contains("unity-skills"))
            {
                // Remove unity-skills related lines
                var lines = content.Split('\n').ToList();
                lines.RemoveAll(l => l.Contains("unity-skills") || l.Trim() == "## UnitySkills");
                
                // Clean up empty consecutive lines
                var cleanedContent = string.Join("\n", lines).Trim() + "\n";
                var utf8NoBom = new UTF8Encoding(false);
                File.WriteAllText(agentsPath, cleanedContent.Replace("\r\n", "\n"), utf8NoBom);
                SkillsLogger.Log("Removed unity-skills from AGENTS.md");
            }
        }

        private static (bool success, string message) UninstallSkill(string targetPath, string name)
        {
            if (!Directory.Exists(targetPath))
                return (false, $"{name} skill not installed at this location");

            Directory.Delete(targetPath, true);
            SkillsLogger.Log("Uninstalled skill from: " + targetPath);
            return (true, targetPath);
        }

        private static (bool success, string message) InstallSkill(string targetPath, string name, string agentId)
        {
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);

            // CRITICAL: Use UTF-8 WITHOUT BOM for Gemini CLI compatibility
            // Gemini CLI cannot parse YAML frontmatter if BOM (EF BB BF) is present at start of file
            var utf8NoBom = new UTF8Encoding(false);

            var skillMd = GenerateSkillMd();
            // Normalize line endings to LF for cross-platform compatibility
            skillMd = skillMd.Replace("\r\n", "\n");
            File.WriteAllText(Path.Combine(targetPath, "SKILL.md"), skillMd, utf8NoBom);

            var pythonHelper = GeneratePythonHelper();
            pythonHelper = pythonHelper.Replace("\r\n", "\n");
            var scriptsPath = Path.Combine(targetPath, "scripts");
            if (!Directory.Exists(scriptsPath))
                Directory.CreateDirectory(scriptsPath);
            File.WriteAllText(Path.Combine(scriptsPath, "unity_skills.py"), pythonHelper, utf8NoBom);

            // Write agent config for automatic agent identification
            var agentConfig = $"{{\"agentId\": \"{agentId}\", \"installedAt\": \"{DateTime.UtcNow:O}\"}}";
            File.WriteAllText(Path.Combine(scriptsPath, "agent_config.json"), agentConfig, utf8NoBom);

            SkillsLogger.Log($"Installed skill to: {targetPath} (Agent: {agentId})");
            return (true, targetPath);
        }

        private static string GenerateSkillMd()
        {
            var sb = new StringBuilder();
            sb.AppendLine("---");
            // Gemini CLI requires: lowercase, alphanumeric and dashes only
            sb.AppendLine("name: unity-skills");
            // CRITICAL: Description must be single-line double-quoted string for Gemini CLI compatibility
            sb.AppendLine("description: \"Unity Editor automation via REST API. Control GameObjects, components, scenes, materials, prefabs, lights, and more with 100+ professional tools.\"");
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("# Unity Editor Control Skill");
            sb.AppendLine();
            sb.AppendLine("You are an expert Unity developer. This skill enables you to directly control Unity Editor through a REST API.");
            sb.AppendLine("Use the Python helper script in `scripts/unity_skills.py` to execute Unity operations.");
            sb.AppendLine();
            sb.AppendLine("## Prerequisites");
            sb.AppendLine();
            sb.AppendLine("1. Unity Editor must be running with the UnitySkills package installed");
            sb.AppendLine("2. REST server must be started: **Window > UnitySkills > Start Server**");
            sb.AppendLine("3. Server endpoint: `http://localhost:8090`");
            sb.AppendLine();
            sb.AppendLine("## Quick Start");
            sb.AppendLine();
            sb.AppendLine("```python");
            sb.AppendLine("# Import the helper from the scripts/ directory");
            sb.AppendLine("import sys");
            sb.AppendLine("sys.path.insert(0, 'scripts')  # Adjust path to skill's scripts directory");
            sb.AppendLine("from unity_skills import call_skill, is_unity_running, wait_for_unity");
            sb.AppendLine();
            sb.AppendLine("# Check if Unity is ready");
            sb.AppendLine("if is_unity_running():");
            sb.AppendLine("    # Create a cube");
            sb.AppendLine("    call_skill('gameobject_create', name='MyCube', primitiveType='Cube', x=0, y=1, z=0)");
            sb.AppendLine("    # Set its color to red");
            sb.AppendLine("    call_skill('material_set_color', name='MyCube', r=1, g=0, b=0)");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("## ⚠️ Important: Script Creation & Domain Reload");
            sb.AppendLine();
            sb.AppendLine("When creating C# scripts with `script_create`, Unity recompiles all scripts (Domain Reload).");
            sb.AppendLine("The server temporarily stops during compilation and auto-restarts.");
            sb.AppendLine();
            sb.AppendLine("```python");
            sb.AppendLine("# After creating a script, check compilation feedback");
            sb.AppendLine("result = call_skill('script_create', name='MyScript', template='MonoBehaviour')");
            sb.AppendLine("if result.get('success') and result.get('compilation', {}).get('isCompiling'):");
            sb.AppendLine("    wait_for_unity(timeout=10)");
            sb.AppendLine("    feedback = call_skill('script_get_compile_feedback', scriptPath=result['path'])");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("## Workflow Examples");
            sb.AppendLine();
            sb.AppendLine("### Create a Game Scene");
            sb.AppendLine("```python");
            sb.AppendLine("# 1. Create ground");
            sb.AppendLine("call_skill('gameobject_create', name='Ground', primitiveType='Plane', x=0, y=0, z=0)");
            sb.AppendLine("call_skill('gameobject_set_transform', name='Ground', scaleX=5, scaleY=1, scaleZ=5)");
            sb.AppendLine();
            sb.AppendLine("# 2. Create player");
            sb.AppendLine("call_skill('gameobject_create', name='Player', primitiveType='Capsule', x=0, y=1, z=0)");
            sb.AppendLine("call_skill('component_add', name='Player', componentType='Rigidbody')");
            sb.AppendLine();
            sb.AppendLine("# 3. Add lighting");
            sb.AppendLine("call_skill('light_create', name='Sun', lightType='Directional', intensity=1.5)");
            sb.AppendLine();
            sb.AppendLine("# 4. Save the scene");
            sb.AppendLine("call_skill('scene_save', scenePath='Assets/Scenes/GameScene.unity')");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("### Create UI Menu");
            sb.AppendLine("```python");
            sb.AppendLine("call_skill('ui_create_canvas', name='MainMenu')");
            sb.AppendLine("call_skill('ui_create_text', name='Title', parent='MainMenu', text='My Game', fontSize=48)");
            sb.AppendLine("call_skill('ui_create_button', name='PlayBtn', parent='MainMenu', text='Play', width=200, height=50)");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("## Available Skills");
            
            // Dynamic Reflection Logic
            var skillsByCategory = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<System.Reflection.MethodInfo>>();
            
            var allTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => { try { return a.GetTypes(); } catch { return new System.Type[0]; } });

            foreach (var type in allTypes)
            {
                foreach (var method in type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                {
                    var attr = System.Reflection.CustomAttributeExtensions.GetCustomAttribute<UnitySkillAttribute>(method);
                    if (attr != null)
                    {
                        var category = type.Name.Replace("Skills", "");
                        if (!skillsByCategory.ContainsKey(category))
                            skillsByCategory[category] = new System.Collections.Generic.List<System.Reflection.MethodInfo>();

                        skillsByCategory[category].Add(method);
                    }
                }
            }

            foreach (var category in skillsByCategory.Keys.OrderBy(k => k))
            {
                sb.AppendLine();
                sb.AppendLine($"### {category}");
                foreach (var method in skillsByCategory[category])
                {
                    var attr = System.Reflection.CustomAttributeExtensions.GetCustomAttribute<UnitySkillAttribute>(method);
                    var skillName = attr.Name ?? method.Name;
                    var description = attr.Description ?? "";
                    
                    var parameters = method.GetParameters()
                        .Select(p => p.Name)
                        .ToArray();
                    var paramStr = string.Join(", ", parameters);
                    
                    sb.AppendLine($"- `{skillName}({paramStr})` - {description}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("## Skill Directory Structure");
            sb.AppendLine();
            sb.AppendLine("```");
            sb.AppendLine("unity-skills/");
            sb.AppendLine("├── SKILL.md          # This file - skill entry point");
            sb.AppendLine("└── scripts/");
            sb.AppendLine("    └── unity_skills.py  # Python helper with call_skill(), is_unity_running(), etc.");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("## Direct REST API");
            sb.AppendLine();
            sb.AppendLine("Direct REST calls can be made to the local UnitySkills server.");
            sb.AppendLine();
            sb.AppendLine("```bash");
            sb.AppendLine("# Health check");
            sb.AppendLine("curl http://localhost:8090/health");
            sb.AppendLine();
            sb.AppendLine("# List all available skills");
            sb.AppendLine("curl http://localhost:8090/skills");
            sb.AppendLine();
            sb.AppendLine("# Execute a skill");
            sb.AppendLine("curl -X POST http://localhost:8090/skill/gameobject_create \\");
            sb.AppendLine("  -H 'Content-Type: application/json' \\");
            sb.AppendLine("  -d '{\"name\":\"MyCube\", \"primitiveType\":\"Cube\"}'");
            sb.AppendLine("```");
            return sb.ToString();
        }

        private static string GenerateAntigravityWorkflow()
        {
            var sb = new StringBuilder();
            sb.AppendLine("---");
            sb.AppendLine("description: Control Unity Editor via REST API. Create GameObjects, manage scenes, components, materials, and more. 100+ automation tools.");
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("# unity-skills");
            sb.AppendLine();
            sb.AppendLine("AI-powered Unity Editor automation through REST API. This workflow enables intelligent control of Unity Editor including GameObject manipulation, scene management, asset handling, and much more.");
            sb.AppendLine();
            sb.AppendLine("## Available modules");
            sb.AppendLine();
            sb.AppendLine("| Module | Description |");
            sb.AppendLine("|--------|-------------|");
            sb.AppendLine("| **gameobject** | Create, modify, find GameObjects |");
            sb.AppendLine("| **component** | Add, remove, configure components |");
            sb.AppendLine("| **scene** | Scene loading, saving, management |");
            sb.AppendLine("| **material** | Material creation, HDR emission, keywords |");
            sb.AppendLine("| **light** | Lighting setup and configuration |");
            sb.AppendLine("| **animator** | Animation controller management |");
            sb.AppendLine("| **ui** | UI Canvas and element creation |");
            sb.AppendLine("| **validation**| Project validation and checking |");
            sb.AppendLine("| **prefab** | Prefab creation and instantiation |");
            sb.AppendLine("| **asset** | Asset import, organize, search |");
            sb.AppendLine("| **editor** | Editor state, play mode, selection |");
            sb.AppendLine("| **console** | Log capture and debugging |");
            sb.AppendLine("| **script** | C# script creation and search |");
            sb.AppendLine("| **shader** | Shader creation and listing |");
            sb.AppendLine("| **workflow** | Time-machine revert, history tracking, auto-save |");
            sb.AppendLine();
            sb.AppendLine("## How to Use");
            sb.AppendLine();
            sb.AppendLine("1. **Check Unity Connection**: Ensure Unity Editor is running with the `SkillsForUnity` plugin.");
            sb.AppendLine("2. **Invoke Skills**: Use `unity_skills.py` (located in the skill's scripts directory) to call Unity functions.");
            sb.AppendLine();
            sb.AppendLine("### Example Prompt");
            sb.AppendLine("`/unity-skills create a red cube at (0, 0, 0)`");
            sb.AppendLine();
            sb.AppendLine("## Best Practices");
            sb.AppendLine();
            sb.AppendLine("- **Save Progress**: Frequently call `scene_save` during automation.");
            sb.AppendLine("- **Undo Support**: Operations are usually undoable in Unity.");
            sb.AppendLine("- **Domain Reload**: Be aware that creating scripts triggers a domain reload.");
            
            return sb.ToString();
        }

        private static string GeneratePythonHelper()
        {
            string helperPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "unity-skills", "scripts", "unity_skills.py"));
            if (!File.Exists(helperPath))
                throw new FileNotFoundException("unity_skills.py template not found", helperPath);

            return File.ReadAllText(helperPath, Encoding.UTF8);
        }
    }
}
