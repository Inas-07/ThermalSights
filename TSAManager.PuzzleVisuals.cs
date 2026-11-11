using ChainedPuzzles;
using ExtraObjectiveSetup.BaseClasses;
using System.Collections.Generic;
using System.Linq;
using ThermalSights.Definition;
using UnityEngine;

namespace ThermalSights
{
    public partial class TSAManager : GenericDefinitionManager<TSADefinition>
    {
        public class PuzzleVisualWrapper
        {
            public GameObject GO { get; set; }

            public Renderer Renderer { get; set; }

            public float Intensity { get; set; }

            public float BehindWallIntensity { get; set; }

            public void SetIntensity(float t)
            {
                if (!GO.active) return;

                if (Intensity > 0.0f)
                {
                    Renderer.material.SetFloat("_Intensity", Intensity * t);
                }

                if (BehindWallIntensity > 0.0f)
                {
                    Renderer.material.SetFloat("_BehindWallIntensity", BehindWallIntensity * t);
                }
            }

            public PuzzleVisualWrapper() { }
        }

        private List<PuzzleVisualWrapper> PuzzleVisuals { get; } = new();

        public void RegisterPuzzleVisual(CP_Bioscan_Core core)
        {
            var components = core.gameObject.GetComponentsInChildren<Renderer>(true);

            if (components != null)
            {
                var renderers = components.Where(comp => comp.gameObject.name.Equals("Zone")).ToList();
                foreach (var r in renderers)
                {
                    var go = r.gameObject;
                    float intensity = r.material.GetFloat("_Intensity");
                    float behindWallIntensity = r.material.GetFloat("_BehindWallIntensity");

                    var wrapper = new PuzzleVisualWrapper()
                    {
                        GO = go,
                        Renderer = r,
                        Intensity = intensity,
                        BehindWallIntensity = behindWallIntensity,
                    };

                    PuzzleVisuals.Add(wrapper);
                }
            }
        }

        public void RegisterPuzzleVisual(PuzzleVisualWrapper wrapper)
        {
            this.PuzzleVisuals.Add(wrapper);
        }

        //private void AddOBSVisualRenderers()
        //{
        //    foreach (var go in EnemyTaggerSettingManager.Current.OBSVisuals)
        //    {
        //        var renderer = go.GetComponentInChildren<Renderer>();
        //        float intensity = renderer.material.GetFloat("_Intensity");
        //        float behindWallIntensity = -1.0f;
        //        PuzzleVisuals.Add(new()
        //        {
        //            GO = go,
        //            Renderer = renderer,
        //            Intensity = intensity,
        //            BehindWallIntensity = behindWallIntensity
        //        });
        //    }
        //}

        public void SetPuzzleVisualsIntensity(float t)
        {
            PuzzleVisuals.ForEach(v => v.SetIntensity(t));
        }

        private void CleanupPuzzleVisuals()
        {
            PuzzleVisuals.Clear();
        }
    }
}
