long_text = open("data.txt")


def challenge_1(text):
    text = text.split("\n")
    game_id = 1
    games = {}
    possibilities = {
        "red": 12,
        "green": 13,
        "blue": 14
    }
    for line in text:
        line = line.split(":")[-1]
        game = {
            "red": 0,
            "green": 0,
            "blue": 0
        }
        for set_ in line.split(";"):
            set_vals = {
                "red": 0,
                "green": 0,
                "blue": 0
            }
            for color in set_.split(","):
                num, _color = color.split()
                set_vals[_color] += int(num)
            for color in game.keys():
                if set_vals[color] > game[color]:
                    game[color] = set_vals[color]

        games[game_id] = game
        game_id += 1
    # print(games)

    possible_games = list(range(1, len(games) + 1))

    for game in games.keys():
        for color in games[game].keys():
            if games[game][color] > possibilities[color]:
                possible_games.remove(game)
                break
    x = 0
    for i in possible_games:
        x += i
    print(f"Challenge 1: {x}")


def challenge_2(text):
    text = text.split("\n")
    power = 0
    for line in text:
        line = line.split(":")[-1]
        game = {
            "red": 0,
            "green": 0,
            "blue": 0
        }
        for set_ in line.split(";"):
            set_vals = {
                "red": 0,
                "green": 0,
                "blue": 0
            }
            for color in set_.split(","):
                num, _color = color.split()
                set_vals[_color] += int(num)
            for color in game.keys():
                if set_vals[color] > game[color]:
                    game[color] = set_vals[color]

        power += game["red"] * game["blue"] * game["green"]

    print(f"Challenge 2: {power}")


long_text = long_text.read()
challenge_1(long_text)
challenge_2(long_text)
