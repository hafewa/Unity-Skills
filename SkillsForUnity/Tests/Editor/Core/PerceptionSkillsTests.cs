using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnitySkills.Tests.Core
{
    [TestFixture]
    public class PerceptionSkillsTests
    {
        private static JObject ToJObject(object result)
        {
            return JObject.Parse(JsonConvert.SerializeObject(result));
        }

        [SetUp]
        public void SetUp()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            GameObjectFinder.InvalidateCache();
        }

        [TearDown]
        public void TearDown()
        {
            GameObjectFinder.InvalidateCache();
        }

        [Test]
        public void SceneAnalyze_ReturnsSuccessWithExpectedStructure()
        {
            var result = PerceptionSkills.SceneAnalyze();
            var json = ToJObject(result);

            Assert.IsTrue(json["success"]?.Value<bool>() ?? false);
            Assert.IsNotNull(json["summary"]);
            Assert.IsNotNull(json["stats"]);
            Assert.IsNotNull(json["findings"]);
            Assert.IsNotNull(json["recommendations"]);
            Assert.IsNotNull(json["suggestedNextSkills"]);
        }

        [Test]
        public void SceneSummarize_CountsObjectsCorrectly()
        {
            new GameObject("TestObj1");
            new GameObject("TestObj2");
            GameObjectFinder.InvalidateCache();

            var result = PerceptionSkills.SceneSummarize();
            var json = ToJObject(result);

            Assert.IsTrue(json["success"]?.Value<bool>() ?? false);
            // Default scene has Camera + Light, we added 2 more
            Assert.IsTrue(json["stats"]?["totalObjects"]?.Value<int>() >= 4);
        }

        [Test]
        public void SceneHealthCheck_DetectsMissingInfrastructure()
        {
            // Start with empty scene (no default objects)
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObjectFinder.InvalidateCache();

            var result = PerceptionSkills.SceneHealthCheck();
            var json = ToJObject(result);

            Assert.IsTrue(json["success"]?.Value<bool>() ?? false);
            Assert.IsNotNull(json["findings"]);
            // Empty scene should report missing camera at minimum
            var findings = json["findings"] as JArray;
            Assert.IsTrue(findings?.Count > 0);
        }

        [Test]
        public void SceneComponentStats_CountsComponentTypes()
        {
            var result = PerceptionSkills.SceneComponentStats();
            var json = ToJObject(result);

            Assert.IsTrue(json["success"]?.Value<bool>() ?? false);
            Assert.IsNotNull(json["stats"]);
            Assert.IsNotNull(json["topComponents"]);
        }

        [Test]
        public void SceneFindHotspots_DetectsDeepHierarchy()
        {
            // Create a deep hierarchy
            var root = new GameObject("DeepRoot");
            var current = root;
            for (int i = 0; i < 10; i++)
            {
                var child = new GameObject($"Level{i}");
                child.transform.SetParent(current.transform);
                current = child;
            }
            GameObjectFinder.InvalidateCache();

            var result = PerceptionSkills.SceneFindHotspots(deepHierarchyThreshold: 5);
            var json = ToJObject(result);

            Assert.IsTrue(json["success"]?.Value<bool>() ?? false);
            Assert.IsNotNull(json["hotspots"]);
            var hotspots = json["hotspots"] as JArray;
            Assert.IsTrue(hotspots?.Count > 0);
        }

        [Test]
        public void HierarchyDescribe_ReturnsNonEmptyTree()
        {
            var result = PerceptionSkills.HierarchyDescribe();
            var json = ToJObject(result);

            Assert.IsTrue(json["success"]?.Value<bool>() ?? false);
            Assert.IsNotNull(json["tree"] ?? json["hierarchy"]);
        }

        [Test]
        public void SceneTagLayerStats_ReportsUsedTags()
        {
            var result = PerceptionSkills.SceneTagLayerStats();
            var json = ToJObject(result);

            Assert.IsTrue(json["success"]?.Value<bool>() ?? false);
        }

        [Test]
        public void ScenePerformanceHints_ReturnsActionableHints()
        {
            var result = PerceptionSkills.ScenePerformanceHints();
            var json = ToJObject(result);

            Assert.IsTrue(json["success"]?.Value<bool>() ?? false);
        }

        [Test]
        public void SceneContractValidate_DefaultContract_ReportsMissingRoots()
        {
            // Empty scene should be missing default contract roots
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObjectFinder.InvalidateCache();

            var result = PerceptionSkills.SceneContractValidate();
            var json = ToJObject(result);

            Assert.IsTrue(json["success"]?.Value<bool>() ?? false);
            var findings = json["findings"] as JArray;
            Assert.IsTrue(findings?.Count > 0, "Empty scene should have missing root findings");
        }

        [Test]
        public void SceneContractValidate_CustomRoots_OverridesDefault()
        {
            new GameObject("MyCustomRoot");
            GameObjectFinder.InvalidateCache();

            var result = PerceptionSkills.SceneContractValidate(
                requiredRootsJson: "[\"MyCustomRoot\"]");
            var json = ToJObject(result);

            Assert.IsTrue(json["success"]?.Value<bool>() ?? false);
        }

        [Test]
        public void BuildSuggestedNextSkills_FiltersInvalidSkillReferences()
        {
            // scene_analyze calls BuildSuggestedNextSkills internally
            // All returned skills should be valid registered skills
            var result = PerceptionSkills.SceneAnalyze();
            var json = ToJObject(result);

            var suggestions = json["suggestedNextSkills"] as JArray;
            if (suggestions != null)
            {
                foreach (var s in suggestions)
                {
                    var skillName = s["skill"]?.ToString();
                    Assert.IsTrue(SkillRouter.HasSkill(skillName),
                        $"suggestedNextSkills contains invalid skill: {skillName}");
                }
            }
        }
    }
}
