namespace Lorekeeper
{
    public enum AssetType
    {
        Null,

        // Audio
        AudioClip,
        AudioMixer,

        // Graphics
        Sprite,
        Texture,
        RenderTexture,
        Cubemap,
        Material,
        Shader,
        ComputeShader,

        // Animation
        AnimationClip,
        AnimatorController,
        Avatar,

        // Models & Prefabs
        Model,
        Mesh,
        Prefab,

        // UI
        Font,
        TMPFontAsset,

        // Data
        ScriptableObject,
        TextAsset,

        // Physics
        PhysicsMaterial,
        PhysicsMaterial2D,

        Other
    }
}