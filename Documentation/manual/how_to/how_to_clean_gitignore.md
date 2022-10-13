# 如何在更新gitignore文件后，自动清理git中的无用文件

**以下的操作是危险的，需要在操作前commit所有的更改，否则会丢失**

1. 移除所有的trached文件

```bash
git rm -r --cached .
```

2. 重新按照新的gitignore文件添加track

```bash
git add .
```

3. commit 这次更改

```bash
git commit -m "fix(gitignore): update .gitignore"
```

4. push 这次更改

```bash
git push -u origin master
```
