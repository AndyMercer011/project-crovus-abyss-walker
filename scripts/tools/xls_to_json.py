import pandas
import argparse
import os

parser = argparse.ArgumentParser()
parser.add_argument("--export_to", type=str, help="json export header")
parser.add_argument("--xls", type=str,
                    help="the source xls configuration file")

'''
    Parse XLS/XLSX file to JSON by row, note that if the rows contains the 
    name/id column, the name/id will be used as the file name, otherwise 
    the index will be used as the file name

    inpuit: xls file
    output_dir: output folder for json file    
'''
def xls_to_json(input: str, output_dir: str) -> None:
    # check input file exist
    if not os.path.exists(input):
        raise FileNotFoundError("[%s] not found" % (input))

    # check outout file location
    if os.path.exists(output_dir):
        if not os.access(output_dir, os.W_OK):
            raise PermissionError(
                "doesn't have write permssion to folder [%s]" % (output_dir))
    else:
        os.makedirs(output_dir)

    # read xls file
    df = pandas.read_excel(input)

    # get the column names
    cols = df.columns.values.tolist()

    # get the data
    for index, row in df.iterrows():
        file_name: str = ''
        if "name" in cols:
            file_name = row["name"]
        elif "id" in cols:
            file_name = row["id"]
        else:
            file_name = str(index)

        with os.open(os.path.join(output_dir, file_name+".json"), "w") as f:
            f.write(row.to_json(orient="index"))

if __name__ == "__main__":
    args = parser.parse_args()
    xls_to_json(args.xls, args.export_to)
