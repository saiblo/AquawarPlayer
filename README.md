# 鱼洋陷役播放器

## 分支说明

- `master` - 主分支，发放给选手的播放器由该分支构建而成
- `final` - 录制决赛解说视频所用的播放器由该分支构建而成

## 开发组成员

- sunx19@mails.tsinghua.edu.cn
- yaowt19@mails.tsinghua.edu.cn
- xu-d19@mails.tsinghua.edu.cn
- zhang-sn19@mails.tsinghua.edu.cn

## WebGL 本地调试提示

将 `replay.json` 放到 `Assets/WebGLTemplates/Template/` 目录下，启动后在浏览器控制台输入指令：

```javascript
fetch("replay.json").then((r) => r.blob()).then((r) => postMessage({message:"init_replay_player", replay_data: r}))
```

测试选手名称的参考代码如下所示：

```javascript
postMessage({message:"load_players", "players": ["A", "B"]})
```