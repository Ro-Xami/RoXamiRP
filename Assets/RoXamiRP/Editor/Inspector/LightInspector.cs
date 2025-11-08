using UnityEngine;
using UnityEditor;

namespace RoXamiRP
{
    [CanEditMultipleObjects]
        [CustomEditor(typeof(Light))]

    public class LightInspector : LightEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!settings.lightType.hasMultipleDifferentValues &&
                (LightType)settings.lightType.enumValueIndex == LightType.Spot)
            {
                settings.DrawInnerAndOuterSpotAngle();
                settings.ApplyModifiedProperties();
            }
        }
    }
}
