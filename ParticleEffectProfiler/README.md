# ParticleEffectProfiler
* Unity特效性能分析工具
* from:https://github.com/jefflopezycj/ParticleEffectProfiler

### 工具说明
该分析工具，主要是把特效的几个重要指标：内存、DrawCall、粒子数量、overdraw等数据显示在scene上，方便美术直接查看，并根据相关内容作出优化。
### 使用说明
- 1. 打开TestEffect场景
![Image text](https://github.com/V1nChy/UnityTools/blob/master/ParticleEffectProfiler/Document/1.png)
- 2. 找到需要测试的特效预设，直接拖入层级面板，将自动运行测试实例。特效默认位置为（0，0，0），如果出现Game窗口无法查看到特效，可自行调整位置。
![Image text](https://github.com/V1nChy/UnityTools/blob/master/ParticleEffectProfiler/Document/2.png)
 
数据说明
1.	在Scene窗口可查看内存、DrawCall、粒子数量、overdraw等数据。
![Image text](https://github.com/V1nChy/UnityTools/blob/master/ParticleEffectProfiler/Document/3.png)
 
2.	在检视面板可查看Overdraw、DrawCall、粒子数量折线图。折线图内一个点代表一帧，如果没有勾选循环，则默认会记录3秒的数据，一秒30帧，会记录90个点。如果是循环特效，就勾选循环，数据会不断记录，但显示的长度默认是90个点。如果存在无法自动裁剪的特效，将在检视面板中显示。
 ![Image text](https://github.com/V1nChy/UnityTools/blob/master/ParticleEffectProfiler/Document/4.png)

3.	可在Game窗口查看Overdraw情况
 ![Image text](https://github.com/V1nChy/UnityTools/blob/master/ParticleEffectProfiler/Document/5.png)

