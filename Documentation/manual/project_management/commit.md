# 关于commit的规则

## commit 格式

```
提交类型(模块):(提交描述)
````

### 接受的提交类型有

* `feat`: 新功能
* `fix`: 修复bug
* `docs`: 文档
* `style`: 格式
* `refactor`: 重构

### 提交描述的要求

1. 提交描述不能为空
2. 提交描述不能超过100个字符
3. 对于需要添加的描述细节，请在提交正文(commit body)中添加，如果不需要可以保持为空
4. 提交正文中不能包含以下字符: `~!@$%^&*=+{}[]|\;'<>?/`
5. 提交正文的每一行不能超过100个字符
6. 文档与代码分开提交，文档不需要过流水会自动跳过

### 提交示例

```
docs(commit_message_example): add example of an good commit message

Capitalized, short (50 chars or less) summary

More detailed explanatory text, if necessary.  Wrap it to about 72
characters or so.  In some contexts, the first line is treated as the
subject of an email and the rest of the text as the body.  The blank
line separating the summary from the body is critical (unless you omit
the body entirely); tools like rebase can get confused if you run the
two together.

Write your commit message in the imperative: "Fix bug" and not "Fixed bug"
or "Fixes bug."  This convention matches up with commit messages generated
by commands like git merge and git revert.

Further paragraphs come after blank lines.

- Bullet points are okay, too

- Typically a hyphen or asterisk is used for the bullet, followed by a
  single space, with blank lines in between, but conventions vary here

- Use a hanging indent

If you use an issue tracker, add a reference(s) to them at the bottom,
like so:

Resolves: #123
```

## commit 流程

  1. 开一个新的Issue并创建对应的MR（选择合并提交）。 如果需要增改对应文档，需要单独开一个新的文档issue，并创建对应的MR。
  2. 在代码的MR中提交代码，在文档的MR中提交文档更改
  3. 等待流水通过后把对应的MR转给@tony.chen424作review
  4. 解决完成所有review的争议后@tony.chen424会把这个文档和代码MR合入到master中

### 注意事项

  1. 需要进行原子提交
  2. 需要有意义的commit message
  3. 提交代码的基本要求是要做到不破坏编译
  4. 如果需要文档的话，需要单独开一个文档issue，并创建对应的MR
