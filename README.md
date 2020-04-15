# Lib.Transform

Batch transformation of config files (.xml + .json) using xdt and jsonpatch

## Installation

dotnet tool install --global Lib.Transform `[`--version`]`

## Syntax

transform `<`rootdir`>` --env `<`environments`>`

- rootdir: full path to root dir of transformations. Transforms are applied in rootdir and all subdirs
- environments: Pipe separated strings of environments. Transformations are applied in order.

### Example

Aim: Apply all transformations in d:\myproject\output for all, azure and prod.

**File structure:**

- myfile.config
- myfile.all.xdt
- myfile.dev.xdt
- myfile.azure.xdt
- myfile.prod.xdt

**Exampe command:** transform d:\myproject\output --env all|azure|prod

**Outcome:**

1. first **myfile.all.xdt** is applied to **myfile.config**
2. then **myfile.azure.xdt** is applied to **myfile.config**
3. then **myfile.prod.xdt** is applied to **myfile.config**

**myfile.dev.xdt** is not applied since dev was not included in the environments parameter
