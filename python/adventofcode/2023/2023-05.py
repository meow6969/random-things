import numpy

long_text = open("data.txt")


def challenge_1(text):
    seeds = []
    information = {}
    current_thing = ""  # i support the current thing !!!
    # interpret information from data
    for line in text:
        if line.startswith("seeds: "):  # seeds is special bcs the data is on same line as the name
            for num in line[7:].split():
                seeds.append(int(num))
        elif ":" in line:
            current_thing = line.strip()
            information[current_thing] = []
        elif line.strip() != "":
            information[current_thing].append(list(map(int, line.split())))

    outputs = []
    for seed in seeds:
        current_input = seed
        for mapz in information.keys():
            for line in information[mapz]:
                if line[1] <= current_input < (line[1] + line[2]):
                    new_input = line[0] + (current_input - line[1])
                    current_input = new_input
                    break
        outputs.append(current_input)
    x = min(outputs)
    print(f"Challenge 1: {x}")


def challenge_2(text):
    seeds = []
    information = {}
    current_thing = ""  # i support the current thing !!!
    # interpret information from data
    for line in text:
        if line.startswith("seeds: "):  # seeds is special bcs the data is on same line as the name
            seed_pair = []
            for num in line[7:].split():
                seed_pair.append(int(num))
                if len(seed_pair) == 1:
                    pass
                else:
                    seeds.append([seed_pair[0], seed_pair[0] + seed_pair[1]])
                    seed_pair = []
                # seeds.append(int(num))
        elif ":" in line:
            current_thing = line.strip()
            information[current_thing] = []
        elif line.strip() != "":
            information[current_thing].append(list(map(int, line.split())))

    outputs = []
    for seed in seeds:
        current_input = seed
        for mapz in information.keys():
            for line in information[mapz]:
                if line[1] <= current_input < (line[1] + line[2]):
                    new_input = line[0] + (current_input - line[1])
                    current_input = new_input
                    break
        outputs.append(current_input)
    # print(information)
    x = min(outputs)
    print(f"Challenge 2: {x}")


long_text = long_text.read().split("\n")
challenge_1(long_text)
challenge_2(long_text)
