import git
import argparse

parser = argparse.ArgumentParser()
parser.add_argument("--repo", type=str, help="Folder path of the repo")

if __name__ == "__main__":
    args = parser.parse_args()
    repo = git.Repo(args.repo)

    # check commit message
    commit_msg = repo.branches["master"]

    # split commit message into title and body
    commit_msg_title = commit_msg[0]
    commit_msg_body = commit_msg[1:]

    # check title length
    if len(commit_msg_title) < 5:
        print("Commit message title is too short")
        exit(-1)
    elif len(commit_msg_title) > 100:
        print("Commit message title is too long")
        exit(-1)
    






    




