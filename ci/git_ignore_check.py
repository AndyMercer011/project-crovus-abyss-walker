import argparse
import subprocess,sys


parser = argparse.ArgumentParser()
parser.add_argument("--repo", type=str, help="Folder path of the repo")

if __name__ == "__main__":
    args = parser.parse_args()
    list_of_files = subprocess.check_output("git ls-files", shell=True, cwd=args.repo).splitlines()

    for file in list_of_files:
        try:
            subprocess.check_call("git check-ignore " + str(file), shell=True, cwd=args.repo)
            sys.exit(-1)
        except subprocess.CalledProcessError:
            pass