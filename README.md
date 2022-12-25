# AnimatorController as a Code

[![openupm][shields-openupm]][openupm-package]
[![GitHub release][shields-latest-release]](https://github.com/anatawa12/AnimatorController-as-a-Code/releases/latest)
[![GitHub deployments][shields-deployment-master]](https://github.com/anatawa12/AnimatorController-as-a-Code/releases/latest)
[![GitHub deployments][shields-deployment-vpm]][vpm-repository]

<!-- 
If it's rejected from shields owners, I'll use custom endpoints.
If it going to be accepted, I'll use the new endpoint. https://github.com/badges/shields/issues/8752
[![VPM release][shields-vpm]][vpm-repository
[shields-vpm]: https://img.shields.io/endpoint?url=https://vpm.anatawa12.com/animator-controller-as-a-code/shields.json
-->

[shields-openupm]: https://img.shields.io/npm/v/com.anatawa12.animator-controller-as-a-code?label=openupm&registry_uri=https://package.openupm.com
[shields-latest-release]: https://img.shields.io/github/v/release/anatawa12/AnimatorController-as-a-Code?display_name=tag&sort=semver
[shields-deployment-vpm]: https://img.shields.io/github/deployments/anatawa12/AnimatorController-as-a-Code/vpm.anatawa12.com?label=VPM%20Deployment
[shields-deployment-master]: https://img.shields.io/github/deployments/anatawa12/AnimatorController-as-a-Code/master%20branch?label=Deployment

A small Unity Editor Library to generate AnimatorController with C# Code.

Inspired by [AV3 Animator as Code] but it depends on VRChat SDK. 
This library is made for vanilla Unity without any libraries.

[AV3 Animator as Code]: https://github.com/hai-vr/av3-animator-as-code

# Installation

## Using OpenUPM

This project is also published on OpenUPM.
See [Package Page on OpenUPM][openupm-package] for Installation steps.


## Using add package from git url

You can add `https://github.com/anatawa12/AnimatorController-as-a-Code.git#<version>` as git-based UPM dependencies.

See [Installing from a Git URL][upm-gui-giiturl] on Unity Documentation for more details.


## Using VPM CommandLine Interface

You may install this package using [VPM/VCC CLI][vcc-cli].
This is recommended step for VRChat Avatars/Worlds Projects.


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

[openupm-package]: https://openupm.com/packages/com.anatawa12.animator-controller-as-a-code/
[upm-gui-giiturl]: https://docs.unity3d.com/Manual/upm-ui-giturl.html
[vcc-cli]: https://vcc.docs.vrchat.com/vpm/cli
[vpm-resolver]: https://vcc.docs.vrchat.com/vpm/resolver
[installer unitypackage]: https://github.com/anatawa12/AnimatorController-as-a-Code/raw/master/.readme/installer.unitypackage
[VPAI]: https://github.com/anatawa12/VPMPackageAutoInstaller
[vpm-repository]: https://vpm.anatawa12.com/vpm.json
