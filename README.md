# Procedural Vegetation Distribution
This repository contains a Unity (v 2020.3.24) project for generating vegetation distribution across a [Unity Terrain](https://docs.unity3d.com/Manual/script-Terrain.html). It is based on placement strategy of this paper by Bruno Torres do Nascimento, Flavio Paulus Franzin, Cesar Tadeu Pozzer, [GPU-Based Real-Time Procedural Distribution of Vegetation on Large-Scale Virtual Terrains](https://www.sbgames.org/sbgames2018/files/papers/ComputacaoFull/188348.pdf), where I attempt to replicate the placement logic. None of the GPU speedups mentioned in the paper are part of this project, and there are minor changes to how for example maps are calculated in some cases. But for the most part, it should be the same map generation and placement strategy as the one used in the paper. 

This project was made for the course DD2470 Advanced Topics in Visualization and Computer Graphics at KTH Royal Institute of Technology.

Progress: 

* I am able to generate: All of the maps! 🥳
  - height map (auto function from unity, thanks)
  - mean height map via `MapCreator` script, function `CreateMeanHeightMap()`
  - relativeHeightMap, also via `MapCreator` script, function `CreateRelativeHeightMap()`
  - slopeMap, via `MapCreator`, function `CreateSlopeMap`. 
  - (waterMap- not really generated just inputte)
  - (waterSpreadMap - yes it is generated but not by me. It's done thanks to catlike coding's SDF plugin)
  - moistureMap
* I can place down plants on the terrain based on the maps! 
  - Poisson disc distribution of positions on the terrain are generated
  - For each position, evaluate a if a plant should be placed using placement algorithm [I.] 
  - Placement algorithm is based on influence curves for each plant, onto the maps generated from the terrain

* Kind of related:
  - I can make L system 2d trees, wow very cool thanks Kanye
 
* To Do:  
  - Demonstration:
     - Include map visuals and the terrain that spawned it (what did I mean by this)
  - Everything else in the paper...
    - plant hierarchy/layering division
    - MORE PLANTS (especially smaller ones for ground coverage)
    - and probably some more i am forgetting right now wow
  
Other things I use: 
* For calculating the relative height map, I use a formula from [Miller, Bradley. (2014). Semantic calibration of digital terrain analysis scale. Cartography and Geographic Information Science. 41. 166-176. 10.1080/15230406.2014.883488.](https://www.researchgate.net/publication/261550103_Semantic_calibration_of_digital_terrain_analysis_scale)

## How to use
1. Create or import a Unity Terrain into your project.
2. Select Window -> Terrain to image to create a heightmap from the terrain. Save it somewhere in your Unity Assets folder. 
3. Create a new object and put the script MapCreator.cs on it.
4. Set it up, some suggested values that will work OK are shown below. Click "GENERATE MAPS", and save them somewhere in your Unity Assets folder. <p align="middle">
    <img src="/Assets/MiscImages/MapCreatorInspector.png" width="256" />
  </p>
  
5. For every plant model you want to use, create a prefab with the Plant script on it, and populate the inspector parameter values. Note that the actual model object containing the mesh that will be placed is the "plant object with mesh", this *must* have a "Plant" tag on it, else it won't be removed when generating a new distribution of plants, and your project will get clogged. <p align="middle">
    <img src="/Assets/MiscImages/ConiferPrefabInspector.png" width="312" />
  </p>
6. Create another new object that will host the PlantController script. Populate inspector parameter values, see below for example. Click place plants to generate a distribution. <p align="middle">
    <img src="/Assets/MiscImages/PlantControllerInspector.png" width="352" />
  </p>

## Showcase

### Maps of the terrain
<p align="left">
  <img src="/Assets/TerrainImages/heightMap.png" width="256" />
  <img src="/Assets/TerrainImages/meanHeightMap.png" width="256" /> 
</p>
<p align="left">
  <img src="/Assets/TerrainImages/relativeHeightMap.png" width="256" />
  <img src="/Assets/TerrainImages/slopeMap.png" width="256" /> 
</p>
<p align="left">
  <img src="/Assets/TerrainImages/waterMap.png" width="256" />
  <img src="/Assets/TerrainImages/waterSpreadMap.png" width="256" /> 
  <img src="/Assets/TerrainImages/moistureMap.png" width="256" /> 
</p>

### Progress of plant placement:
<p align="left">
  <img src="/Assets/MiscImages/wrong1.png" width="512" />
</p>
<p align="left">
  <img src="/Assets/MiscImages/wrong2.png" width="512" /> 
</p>
<p align="left">
  <img src="/Assets/MiscImages/right1.png" width="512" /> 
</p>
<p align="left">
  <img src="/Assets/MiscImages/final_pic_1.png" width="512" /> 
</p>
