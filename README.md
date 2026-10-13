# Kingdom Pxels

2D 横版像素平台原型。当前场景是一段森林关卡：角色可以移动、跳跃、三段近战连招和翻滚，小怪会巡逻并可以被打掉，玩家有生命值与心形界面。

## 环境

- 团结引擎 1.10.3（编辑器版本 `2022.3.62t15`）
- Universal Render Pipeline 2D

用团结引擎打开本目录下的工程，进入 `Assets/Scenes/SampleScene.scene` 后点播放。

## 操作

| 按键 | 作用 |
| --- | --- |
| A / D 或方向键 | 左右移动 |
| 空格 | 跳跃；松开可以短跳 |
| J | 地面三段连招 |
| L | 翻滚，攻击中也可以打断 |
| H | 测试扣血（临时调试键） |

## 仓库里有什么

提交的是工程源文件：`Assets`、`Packages`、`ProjectSettings`。

`Library`、`Temp`、`Logs`、`UserSettings`、IDE 工程文件，以及本机的 `.codely-cli` 不会上传。这些是编辑器生成或只在本机使用的内容，克隆后由引擎重新生成即可。
