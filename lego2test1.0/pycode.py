import os
import re

# Path to the directory containing your files
directory_path = r"C:\Users\rohandassani\AzureMigrateRepos\rohan-lego-test2\Target"

# Regular expression to extract fileno from filenames
pattern = re.compile(r'processed_.*_(\d+)\.txt$')

# Collect all file numbers
file_numbers = []

for filename in os.listdir(directory_path):
    match = pattern.match(filename)
    if match:
        file_numbers.append(int(match.group(1)))

# Find missing numbers to see missed events
if file_numbers:
    file_numbers = sorted(file_numbers)
    full_range = set(range(file_numbers[0], file_numbers[-1] + 1))
    missing_numbers = full_range - set(file_numbers)

    if missing_numbers:
        print("Missing file numbers:", sorted(missing_numbers))
    else:
        print("No missing file numbers!")
else:
    print("No valid files found in the directory.")

name = 'C:UsersrohandassaniAzureMigrateReposrohan-lego-test2Sourcetesttestfile_0.txt'
print(len(name))
