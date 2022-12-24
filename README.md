# AnimatorController as a Code

A small Unity Editor Library to generate AnimatorController with C# Code.

Inspired by [AV3 Animator as Code] but it depends on VRChat SDK. 
This library is made for vanilla Unity without any libraries.

[AV3 Animator as Code]: https://github.com/hai-vr/av3-animator-as-code

# Installation

## Using VPM CommandLine Interface

You may install this package using [VPM/VCC CLI][vcc-cli].
This is recommended step for VRChat Avatars/Worlds Projects.

[vcc-cli]: https://vcc.docs.vrchat.com/vpm/cli

```bash
# add our vpm repository
vpm add repo https://vpm.anatawa12.com/vpm.json
# add package to your project
cd /path/to/your-unity-project
vpm add package com.anatawa12.animator-controller-as-a-code
```

## Using Installer UnityPackage with VPM

With [VPAI] You can include this package with

1. download installer unitypackag [here][installer unitypackage].
2. Make sure your project contains [`vpm-resolver`][vpm-resolver]. If your project is VRChat Avatars/Worlds project with VCC, It's installed.
3. Import the unitypackage into your project.

[vpm-resolver]: https://vcc.docs.vrchat.com/vpm/resolver
[installer unitypackage]: https://github.com/anatawa12/AnimatorController-as-a-Code/raw/master/.readme/installer.unitypackage
[VPAI]: https://github.com/anatawa12/VPMPackageAutoInstaller

## Using add package from git url

You can add `https://github.com/anatawa12/AnimatorController-as-a-Code.git#<version>` as git-based UPM dependencies.

See [Installing from a Git URL][upm-gui-giiturl] on Unity Documentation for more details.

[upm-gui-giiturl]: https://docs.unity3d.com/Manual/upm-ui-giturl.html

## Using OpenUPM

This project is also published on OpenUPM.
See [Package Page on OpenUPM][openupm-package] for Installation steps.

[openupm-package]: https://openupm.com/packages/com.anatawa12.animator-controller-as-a-code/
