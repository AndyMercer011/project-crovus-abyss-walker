import argparse
import subprocess

parser = argparse.ArgumentParser()
parser.add_argument("--repo", type=str, help="Folder path of the repo")

if __name__ == "__main__":
    # get the modified files between current branch and master
    args = parser.parse_args()
    modifited_files = subprocess.check_output(
        "git --no-pager diff --name-only origin/master", shell=True, cwd=args.repo).splitlines()

    # check if the modified files are in the docs folder
    for file in modifited_files:
        if ("docs/" not in str(file)) or ("docs\\" not in str(file)):
            print("Should only modify docs folder when commit message containes 'docs'")
            exit(-1)