# Empty Mall Asset Package

Imported from `Empty_Mall.usdz` without interrupting or modifying any existing scene, code, or asset in the project.

## Contents

- **`Empty_Mall.usdz`**: The original USDZ model uploaded to the project.
- **`Empty_Mall.obj` & `Empty_Mall.mtl`**: Universal Wavefront 3D model natively supported by Unity without requiring any external USD/glTF plugins.
- **`EmptyMall.prefab`**: Pre-configured Unity prefab with:
  - Mesh Filter & Mesh Renderer
  - `EmptyMallMaterial` pre-assigned
  - Mesh Collider pre-attached for physics and player collision
- **`Materials/EmptyMallMaterial.mat`**: Unity Standard Shader PBR material.
- **`Textures/`**:
  - `Empty_Mall_BaseColor.jpg`: Albedo texture map.
  - `Empty_Mall_Normal.jpg`: Tangent space normal map.
  - `Empty_Mall_MetallicSmoothness.png`: Combined metallic (RGB) and smoothness (Alpha = 1 - Roughness) map formatted for Unity's Standard Shader.
  - `Empty_Mall_Metallic.jpg`: Raw metallic map.
  - `Empty_Mall_Roughness.jpg`: Raw roughness map.
  - `Empty_Mall_Emissive.jpg`: Emission map.

## How to Use in Unity

1. **Drag-and-Drop**: Simply drag `Assets/EmptyMall/EmptyMall.prefab` into any scene hierarchy.
2. **Transform**: The mesh is modeled at 1:1 real-world meter scale (~120m wide x 22.3m high x 100m deep).
3. **Collision**: A MeshCollider is already enabled so characters and raycasts will interact with the floors and walls automatically.
