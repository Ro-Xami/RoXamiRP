namespace RoXamiRP
{
    public abstract class RoXamiSceneWindowBase
    {
        public bool IsActive { get; set; }
        
        public abstract void OnEnable();

        public abstract void OnSceneView(float width, float height);
    }
}