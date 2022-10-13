# 项目的流水(Precheckin)

目前项目的流水包括以下的stage：

* pre_merge_check(预合并检查,只会在MR的合并前进行)

* build_and_test(构建和测试, 目前还未添加)

## pre_merge_check

在这个阶段会做如下的检查：

* 检查是否有符合gitinore的文件被加入到repo中
  
* 根据[commit规则](commit.md)检查commit message

* 如果修改了docs/目录下的文件检查这次提交是否只包含文档更新
