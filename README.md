# dd2470project
Probably going to include procedural plant generation and placement

Easy mode: L system for plants, poisson disc sampling for procedural placement

Hard mode: "Synthetic Silviculture: Multi-scale Modeling of Plant Ecosystems" for both, OR the brazilian 2018 paper for placement combined with either L system generated plants or existing models

So far I am trying to replicate the placement strategy of this paper: B. Torres do Nascimento, C. Pozzer, [GPU-Based Real-Time Procedural Distribution of Vegetation on Large-Scale Virtual Terrains](https://www.sbgames.org/sbgames2018/files/papers/ComputacaoFull/188348.pdf ), 17th Brazilian Symposium on Computer Games and Digital Entertainment, 2018

Progress: 

* I am able to generate:
  - height map (auto function from unity, thanks)
  - mean height map via MapCreator script, function CreateMeanHeightMap()

* Kind of related:
  - I can make L system 2d trees, wow very cool thanks Kanye
  
Other things I use: 
* For calculating the relative height map, I use a formula from [Miller, Bradley. (2014). Semantic calibration of digital terrain analysis scale. Cartography and Geographic Information Science. 41. 166-176. 10.1080/15230406.2014.883488.](https://www.researchgate.net/publication/261550103_Semantic_calibration_of_digital_terrain_analysis_scale)
