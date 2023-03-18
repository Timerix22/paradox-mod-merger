# Paradox Mod Merger
A piece of shitty code i wrote to simplify merging Paradox Interacive's game modifications. 
Works for HOI4 and Stellaris.  

## Installation
1. install 7z and add it to path
2. install dotnet7.0 sdk  
3. ```sh
   cp Makefile some_mod_colletion_dir/
   ```
4. set `CSPROJ_DIR` and `STEAMLIB_DIR` in `Makefile`
5. ```shell
   make update_merger
   ```
6. ```shell
   make create_dirs
   ```


## dir structure
```xpath
separate/ mods sorted by categories
├── content/ 
├── graphics/
├── localisation/
├── post/
├── pre/
├── ui/
└── unused/                 
src/                      
├── src_yyy.MM.dd_hh-mm-ss/  
│   └── ... unsorted mods copied from steam workshop copied by clean_workshop task   
└── ...  
merged/
└── ... merged mod categories
````
