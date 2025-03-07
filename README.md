# Lib.Transform

Batch transformation of config files (.xml + .json) using xdt and jsonpatch

## Installation

dotnet tool install --global Lib.Transform \[--version\]

## Syntax

transform  \<rootdir\> --env \<environments\>

- rootdir: full path to root dir of transformations. Transforms are applied in rootdir and all subdirs
- environments: Pipe separated strings of environments. Transformations are applied in environment order.

## Example

Aim: Apply all transformations in d:\myproject\output for all, azure and prod.

**Folder structure:**

- **myfile.config**
- myfile.all.xdt
- myfile.dev.xdt
- myfile.azure.xdt
- myfile.prod.xdt
- **myfile.json**
- myfile.all.jsonpatch
- myfile.dev.jsonpatch
- myfile.azure.jsonpatch
- myfile.prod.jsonpatch

**Command:** transform d:\myproject\output --env all|azure|prod

**Application order:**

1. first **myfile.all.xdt** is applied to **myfile.config**
2. then **myfile.all.jsonpatch** is applied to **myfile.json**
3. then **myfile.azure.xdt** is applied to **myfile.config**
4. then **myfile.azure.jsonpatch** is applied to **myfile.json**
5. then **myfile.prod.xdt** is applied to **myfile.config**
6. then **myfile.prod.jsonpatch** is applied to **myfile.json**

**myfile.dev.xdt** and **myfile.dev.jsonpatch** are not applied since **dev** was not included in the environments parameter
