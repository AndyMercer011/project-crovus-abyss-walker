# Git-LFS专题

## 如何添加新的文件到Git-LFS上

添加文件：

```bash
git lfs track "example.png"
```

添加一个新的规则：

```bash
git lfs track "*.png"
```

commit这个修改：

```bash
git commit -m "fix(gitlfs): update gitlfs files"
```

## 如何把已有的文件应用Git-LFS的规则

添加文件：

```bash
git add --renormalize .
```

重新commit修改：

```bash
git commit -m "fix(gitlfs): update gitlfs files"
```
