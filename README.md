# CORE Cinema
![](https://img.shields.io/badge/2025--03--18-0.1.6-green)

See [CORE Confluence](https://github.com/CC-Open-Learning/CORE-confluence) documentation for developer's notes.

A **CORE Framework** package providing camera controls build on top of the [Cinemachine](https://docs.unity3d.com/Packages/com.unity.cinemachine@2.3/manual/index.html) Unity package. Refactors the **CORECameraController** built for COREv1.

Full documentation for [camera controls with CORE Cinema](https://varlab-dev.atlassian.net/wiki/spaces/CV2/pages/527171585/Camera+Controls) can be found in the **CORE Framework** Confluence space.



## Installation

### Package Manager
**CORE Cinema** can be found in the [CORE UPM Registry](http://upm.core.varlab.org:4873/) as `com.varlab.corecinema` 📦

Navigate to the **Package Manager** window in the Unity Editor and install the package under the **My Registries** sub-menu.


### Legacy Installation
In the `Packages/manifest.json` file of the Unity project project, add the following line to dependencies:

`"com.varlab.corecinema": "ssh://git@bitbucket.org/VARLab/core-cinema.git#upm"`

Optionally, replace `upm` with a specific commit tag such as `0.1.6` to track a specific package version.