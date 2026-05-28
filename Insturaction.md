# Project Specification: The Email Distraction Space
## Introduction
This technical specification outlines the design and implementation of an immersive, non-interactive Unity experience space. The project serves as an audiovisual metaphor for contemporary digital anxiety, specifically focusing on how the continuous influx of corporate communications fragments human attention.
To maintain academic rigor and strict project constraints, the entire scene is constructed using **Unity's native 3D primitive objects** paired with procedural systems. This approach eliminates the need for external modeling software while establishing a polished, minimalist cubist aesthetic reminiscent of *Minecraft* combined with modern digital installation art.
## 1. Core Conceptual Framework
The installation frames user attention as an organic, living ecosystem that is systematically dismantled by a relentless, automated grid.
### 1.1 The Visual Metaphor
 * **The Focus Tree (Organic System):** Represents the user's cognitive bandwidth and state of flow. Built out of nested geometric configurations, it begins in a vibrant, balanced state.
 * **The Email Blocks (Inorganic Grid):** Represent external disruptions. They are rigid, heavy, and uniform cubes containing high-contrast colors that forcefully break the organic composition of the space.
### 1.2 Narrative Arc (Timeline Phases)
The experience runs on a linear timeline, moving through three distinct emotional states:
| Phase | Title | Visual State | Audio Profile |
|---|---|---|---|
| **Phase I** | *The State of Flow* | Vibrant green tree growing in a dark, minimalist void. | Ambient white noise, low-frequency lo-fi chords. |
| **Phase II** | *The Intrusive Influx* | White and blue cubes rain down, physically cluttering the branches. | Intermittent, high-pitched system notification pings. |
| **Phase III** | *Cognitive Suffocation* | The tree is buried under a mountain of cubes; textures turn pitch black. | Glitched audio loops, high-frequency static, sudden silence. |
## 2. Technical Scene Construction
```
[Hierarchy Layout]
├── Main Camera (Static, Orthographic/Low-FoV Perspective)
├── Directional Light (Dynamic Color/Intensity)
├── Audio Manager (Ambient & SFX Tracks)
├── Experience_Timeline (Director)
└── _Scene_Environment
    ├── Focus_Tree (Root)
    │   ├── Trunk (Nested Cylinders/Cubes)
    │   └── Leaves_Group (Array of Scale-Controlled Cubes)
    └── Email_Spawner (Scripted Transform Grid)

```
### 2.1 The Focus Tree Assembly
The tree is constructed entirely via the Unity Hierarchy using nested primitive objects to form a stylized low-poly silhouette:
```
Focus_Tree (Empty GameObject)
  ├── Trunk_Base (Cube: Scale 1.5, 4.0, 1.5)
  ├── Branch_Left (Cube: Rotation 0, 0, 35)
  ├── Branch_Right (Cube: Rotation 0, 0, -35)
  └── Leaves_Container (Empty GameObject)
        ├── Leaf_Cube_01 (Cube: Scale 2, 2, 2)
        ├── Leaf_Cube_02 (Cube: Scale 1.5, 1.5, 1.5)
        └── [Additional Leaf Cubes...]

```
### 2.2 The Email Prefab Structural Breakdown
The email is an instanced, physics-enabled primitive composite designed to feel heavy and intrusive:
 * **Envelope Base:** 3D Object -> Cube. Scale adjusted to X=1.6, Y=1.0, Z=0.2 to mimic a rigid, sleek digital card. Material is set to a flat, unlit white.
 * **Unread Badge:** 3D Object -> Sphere. Scale adjusted to X=0.2, Y=0.2, Z=0.2. Material utilizes a high-emission crimson red. It is parented to the Envelope Base at a local position offset of X=0.6, Y=0.3, Z=-0.11.
 * **Physics Components:**
   * Box Collider: Extends slightly beyond the mesh boundaries to prevent clipping during high-velocity physics calculations.
   * Rigidbody: Mass set to 2.0 (giving it a heavy, concussive impact when colliding with the leaf nodes). Collision Detection set to Continuous.
## 3. Core Systems Implementation
### 3.1 Procedural Spawning System
To simulate a mounting data influx without killing CPU performance, a lightweight spawning script instantiates the Email Prefab within a bounding volume above the tree canopy.
```csharp
using UnityEngine;

public class EmailSpawner : MonoBehaviour
{
    [Header("Spawn Configuration")]
    public GameObject emailPrefab;
    public Vector3 spawnZoneSize = new Vector3(5f, 1f, 5f);
    
    [Header("Dynamic Timing")]
    public float initialSpawnRate = 2.0f; // Seconds between spawns
    public float minimumSpawnRate = 0.05f;
    public float accelerationRate = 0.95f; // Multiplier over time

    private float _timer;
    private float _currentInterval;
    private bool _isSpawning = false;

    void Start()
    {
        _currentInterval = initialSpawnRate;
        _timer = _currentInterval;
    }

    void Update()
    {
        if (!_isSpawning) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0)
        {
            SpawnEmail();
            // Exponentially decrease interval to simulate a runaway inbox
            _currentInterval = Mathf.Max(minimumSpawnRate, _currentInterval * accelerationRate);
            _timer = _currentInterval;
        }
    }

    private void SpawnEmail()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnZoneSize.x / 2, spawnZoneSize.x / 2),
            Random.Range(-spawnZoneSize.y / 2, spawnZoneSize.y / 2),
            Random.Range(-spawnZoneSize.z / 2, spawnZoneSize.z / 2)
        );

        Vector3 spawnPosition = transform.position + randomOffset;
        Quaternion randomRotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), 0);
        
        Instantiate(emailPrefab, spawnPosition, randomRotation);
    }

    public void SetSpawning(bool state)
    {
        _isSpawning = state;
    }
}

```
### 3.2 Material Decay Shader (Universal Render Pipeline - Shader Graph)
The progressive decay of the tree is driven by global material parameters exposed to Unity Timeline. The transition from organic growth to digital corruption is handled by interpolating color and surface property nodes.
```
[Timeline Animation Track] 
       │
       ▼
[Material: Mat_Tree_Decay]
       │
       ├── Lerp Base Color: (Healthy Emerald Green) ──► (Dead Charcoal Grey)
       ├── Lerp Smoothness: (0.4 Semi-Gloss) ─────────► (0.0 Completely Matte)
       └── Increase Emission: (0.0 Neutral) ──────────► (1.2 Glitch Red Glow)

```
### 3.3 Visual Decay via Node Atrophy
To mirror the classic structural degradation of *Minecraft* leaves while avoiding complex modeling scripts, the leaf blocks utilize a procedural scale reduction script triggered by timeline events:
```csharp
using UnityEngine;

public class LeafAtrophy : MonoBehaviour
{
    public Transform leavesContainer;
    [Range(0f, 1f)] public float decayProgress = 0f;

    private Transform[] _leafNodes;
    private Vector3[] _initialScales;

    void Start()
    {
        int childCount = leavesContainer.childCount;
        _leafNodes = new Transform[childCount];
        _initialScales = new Vector3[childCount];

        for (int i = 0; i < childCount; i++)
        {
            _leafNodes[i] = leavesContainer.GetChild(i);
            _initialScales[i] = _leafNodes[i].localScale;
        }
    }

    void Update()
    {
        for (int i = 0; i < _leafNodes.Length; i++)
        {
            // Staggered decay based on hierarchy index to simulate irregular wilting
            float staggeredThreshold = (float)i / _leafNodes.Length;
            
            if (decayProgress >= staggeredThreshold)
            {
                float localProgress = (decayProgress - staggeredThreshold) / (1f - staggeredThreshold);
                _leafNodes[i].localScale = Vector3.Lerp(_initialScales[i], Vector3.zero, localProgress);
            }
        }
    }
}

```
## 4. Post-Processing and Sensory Overload
To emphasize cognitive fatigue in a purely passive experience, Unity's Post-Processing Volume is automated via Timeline to mimic the physiological symptoms of a panic attack or distraction loop.
### 4.1 Post-Processing Profile Evolution
```
[Phase I: Flow]         [Phase II: Intrusive Influx]         [Phase III: Suffocation]
Vignette: 0.2           Vignette: 0.45                       Vignette: 0.65 (Heavy Heavy Heavy)
Chromatic Ab.: 0.0      Chromatic Ab.: 0.35                  Chromatic Ab.: 1.0 (Color Splitting)
Lens Distortion: 0.0    Lens Distortion: -10                 Lens Distortion: -45 (Claustrophobic)

```
 * **Chromatic Aberration:** As the timeline transitions into Phase III, the color channels split at the periphery of the screen. This optical distortion realistically simulates eye strain and the inability to maintain visual focus.
 * **Lens Distortion:** Gradual shift to a negative intensity value creates a warping "barrel effect," making the room feel like it's collapsing inward on the dying tree.
### 4.2 Spatial Sound Design
Sound is the primary tool used to capture attention when there's no user interaction.
 * **The Localization Disconnect:** Notification pings are detached from the visual spawner. Using a 3D Audio Source with spatial blending set to 1.0 (Full 3D), the pings generate at random coordinates *behind* and *above* the virtual camera. Even without interactive controls, the real-world listener will experience subconscious cognitive shifts as their auditory attention is jerked around the physical room.
# 项目技术说明书：邮件分心体验空间（中文对照）
## 引言
本技术说明书阐述了一个基于 Unity 引擎构建的、以非交互为核心的沉浸式体验空间设计方案。该项目旨在通过视听语言的具象化，表现现代人在面对无休止的职场数字化沟通（如邮件轰炸）时，注意力被强行剥夺的焦虑与无力感。
为了保证学术严谨性并严格遵守项目工程限制，全场建筑及道具**完全基于 Unity 原生 3D 基础几何体（Primitive Objects）**构建，结合程序化系统控制演变。这种方法在完全不依赖外部建模软件的前提下，营造出了一种介于《我的世界（Minecraft）》像素风与现代立体主义装置艺术之间的、极具高级感的极简视觉风格。
## 1. 核心概念框架
本装置将人类的“专注力”视为一个有机的生命生态系统，而将外界的干扰视为一种冰冷、僵硬、带有统治色彩的数字化网格，两者在空间中发生物理碰撞与侵蚀。
### 1.1 视觉隐喻
 * **专注树（有机生态系统）：** 代表用户的认知带宽和心流状态。由错落有致的几何立方体拼装而成，初始状态充满生命力与微光呼吸感。
 * **邮件方块（无机规则网格）：** 代表外界打扰。形体僵硬、冰冷、沉重，带有 Office 办公软件标志性的高饱和度反差色彩，用工业化的硬边缘无情摧毁原有的画面和谐。
### 1.2 叙事全周期（时间轴阶段划分）
体验完全运行在 Unity Timeline（时间轴）上，无需用户操作，通过以下三个阶段递进引发情感共鸣：
| 阶段 | 主题 | 视觉表现 | 声音设计 |
|---|---|---|---|
| **第一阶段** | *心流状态* | 翠绿色的几何树在极简的深色虚空微光中缓慢舒展生长。 | 柔和的自然白噪音，低频空灵的低保真（Lo-fi）和弦。 |
| **第二阶段** | *数字入侵* | 大量蓝白相间的邮件方块自上空坠落，沉重地砸在并堆积在树枝上。 | 间歇性出现、语调逐渐变快且尖锐的系统邮件提示音。 |
| **第三阶段** | *认知窒息* | 邮件方块堆积如山，彻底将树活埋；大树材质变黑，画面出现故障艺术（Glitch）。 | 音乐完全扭曲断裂，高频耳鸣声与红石电路过载杂音交织。 |
## 2. 技术场景构建
```
[层级结构布局 - Hierarchy]
├── Main Camera (静态正交或低视野透视相机)
├── Directional Light (动态色彩与光强控制)
├── Audio Manager (负责背景音与空间音效切换)
├── Experience_Timeline (核心时间轴控制器)
└── _Scene_Environment
    ├── Focus_Tree (专注树根节点)
    │   ├── Trunk (层叠的原生圆柱体/立方体树干)
    │   └── Leaves_Group (受脚本缩放控制的叶片方块组)
    └── Email_Spawner (邮件程序化生成器定位点)

```
### 2.1 专注树的积木式搭建
在 Unity Hierarchy 中完全通过嵌套基础几何体拼装出具有几何美感的低多边形树木轮廓：
```
Focus_Tree (空物体)
  ├── Trunk_Base (立方体: 缩放 1.5, 4.0, 1.5)
  ├── Branch_Left (立方体: 旋转 0, 0, 35)
  ├── Branch_Right (立方体: 旋转 0, 0, -35)
  └── Leaves_Container (叶片容器空物体)
        ├── Leaf_Cube_01 (立方体: 缩放 2, 2, 2)
        ├── Leaf_Cube_02 (立方体: 缩放 1.5, 1.5, 1.5)
        └── [其余原生几何立方体叶片...]

```
### 2.2 邮件预制体（Prefab）结构拆解
邮件被设计为具有物理实体的沉浸重物，砸落时需具备沉闷的撞击感：
 * **信封主体：** 3D Object -> Cube。缩放比例（Transform Scale）调整为：X=1.6, Y=1.0, Z=0.2。使其呈现为一张扁平、硬朗的数字卡片。材质（Material）使用完全不吸光的无光纯白（Unlit White）。
 * **未读红点标志：** 3D Object -> Sphere。缩放比例调整为：X=0.2, Y=0.2, Z=0.2。材质开启强烈的自发光（Emission），设为警示性的深红色。将其拖为信封主体的子物体（Child），并贴在信封右上角表面（局部坐标偏移：X=0.6, Y=0.3, Z=-0.11）。
 * **物理组件配置：**
   * Box Collider（盒状碰撞体）：边界稍微超出网格一点，防止高频下落时产生穿模。
   * Rigidbody（刚体）：质量（Mass）设为 2.0（增强砸向树枝时的物理冲击感），碰撞检测（Collision Detection）设为 Continuous（连续检测，防止漏碰）。
## 3. 核心功能系统实现
### 3.1 程序化邮件生成系统（Spawner）
为了模拟后期“邮件暴雪”的疯狂状态，同时不牺牲 CPU 性能，编写轻量级脚本，在树冠上方的三维箱体空间内随机实例化邮件预制体。
```csharp
using UnityEngine;

public class EmailSpawner : MonoBehaviour
{
    [Header("生成配置")]
    public GameObject emailPrefab;
    public Vector3 spawnZoneSize = new Vector3(5f, 1f, 5f);
    
    [Header("动态时间轴控制")]
    public float initialSpawnRate = 2.0f; // 初始生成间隔（秒）
    public float minimumSpawnRate = 0.05f; // 最高频率极限
    public float accelerationRate = 0.95f; // 每一轮生成的频率缩短系数

    private float _timer;
    private float _currentInterval;
    private bool _isSpawning = false;

    void Start()
    {
        _currentInterval = initialSpawnRate;
        _timer = _currentInterval;
    }

    void Update()
    {
        if (!_isSpawning) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0)
        {
            SpawnEmail();
            // 指数级缩减生成间隔，模拟全面失控的收件箱
            _currentInterval = Mathf.Max(minimumSpawnRate, _currentInterval * accelerationRate);
            _timer = _currentInterval;
        }
    }

    private void SpawnEmail()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnZoneSize.x / 2, spawnZoneSize.x / 2),
            Random.Range(-spawnZoneSize.y / 2, spawnZoneSize.y / 2),
            Random.Range(-spawnZoneSize.z / 2, spawnZoneSize.z / 2)
        );

        Vector3 spawnPosition = transform.position + randomOffset;
        // 让方块产生随机的角度旋转，落地堆积时形态更杂乱、更具压迫感
        Quaternion randomRotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), 0);
        
        Instantiate(emailPrefab, spawnPosition, randomRotation);
    }

    // 由 Timeline 事件触发开启或关闭生成系统
    public void SetSpawning(bool state)
    {
        _isSpawning = state;
    }
}

```
### 3.2 树体材质枯萎着色器（URP Shader Graph）
大树的逐渐衰败和数字侵蚀不通过模型动画实现，而是通过全局材质参数与 Unity Timeline 绑定进行动态插值（Lerp）。
```
[Timeline 动画轨道] 
       │
       ▼
[大树通用材质: Mat_Tree_Decay]
       │
       ├── 基础颜色插值 (Base Color Lerp): (健康翠绿) ──► (死寂焦黑)
       ├── 光滑度插值 (Smoothness Lerp): (0.4 质感反光) ──► (0.0 干燥粗糙)
       └── 故障红光发光 (Emission): (0.0 无自发光) ────► (1.2 警示红光闪烁)

```
### 3.3 原生方块体积萎缩系统（模拟凋零效果）
为了完美模拟《我的世界》中叶片方块失去木材支持后“啪啪啪”碎裂消失的效果，且避免使用复杂的粒子烘焙，利用脚本控制叶片群组的缩放值逐步归零：
```csharp
using UnityEngine;

public class LeafAtrophy : MonoBehaviour
{
    public Transform leavesContainer;
    [Range(0f, 1f)] public float decayProgress = 0f; // 暴露给 Timeline 的百分比参数

    private Transform[] _leafNodes;
    private Vector3[] _initialScales;

    void Start()
    {
        int childCount = leavesContainer.childCount;
        _leafNodes = new Transform[childCount];
        _initialScales = new Vector3[childCount];

        for (int i = 0; i < childCount; i++)
        {
            _leafNodes[i] = leavesContainer.GetChild(i);
            _initialScales[i] = _leafNodes[i].localScale;
        }
    }

    void Update()
    {
        for (int i = 0; i < _leafNodes.Length; i++)
        {
            // 基于方块在 Hierarchy 中的层级索引制造阶梯式延迟，形成错落有致的零星凋零感
            float staggeredThreshold = (float)i / _leafNodes.Length;
            
            if (decayProgress >= staggeredThreshold)
            {
                float localProgress = (decayProgress - staggeredThreshold) / (1f - staggeredThreshold);
                // 方块逐渐缩小至 0，模拟枯萎爆裂消失
                _leafNodes[i].localScale = Vector3.Lerp(_initialScales[i], Vector3.zero, localProgress);
            }
        }
    }
}

```
## 4. 后处理特效（Post-Processing）与感官过载
由于用户在整个过程中是纯被动观看的，因此必须通过极具攻击性的**后处理特效调整**，来外显化人在注意力支离破碎时的生理不适与幽闭感。
### 4.1 后处理参数进化轴（Timeline 驱动）
```
[第一阶段：专注心流]       [第二阶段：杂讯入侵]       [第三阶段：精神窒息]
暗角 (Vignette): 0.2     暗角 (Vignette): 0.45    暗角 (Vignette): 0.65 (视野极度受限)
色差 (Chrom. Ab.): 0.0   色差 (Chrom. Ab.): 0.35  色差 (Chrom. Ab.): 1.0 (色彩严重分裂)
镜头畸变 (Distort): 0.0   镜头畸变 (Distort): -10  镜头畸变 (Distort): -45 (空间向内塌陷)

```
 * **色差特效（Chromatic Aberration）：** 随着第三阶段的到来，画面边缘的红蓝绿通道发生剧烈折射和错位。这能够直接模拟人眼在极度疲劳、焦虑、无法集中注意力时的视觉重影状态。
 * **镜头畸变（Lens Distortion）：** 将参数动态调至极低的负值，画面会产生强烈的向内凹陷扭曲。配合满屏堆积的邮件，会让观者产生空间正在向自己逼近的窒息感与幽闭体验。
### 4.2 空间音频的“注意力剥离”设计
声音是纯观赏体验里最强有力的叙事武器。
 * **声像分离错觉（The Spatial Disconnect）：** 邮件落地虽有闷响，但最核心的“未读提示音”并不从树上方发出。通过将 3D Audio Source 的空间混合度（Spatial Blend）拉满（1.0），让提示音随机在相机的**正后方、耳边、或者头顶上方**生成。人类的听觉本能会在声音响起的瞬间，强迫大脑去定位声源（哪怕身体无法操作），从而在物理现实的空间中，让屏幕前的观者达成真正的“分心”和烦躁感。
