import math
import copy


long_text = open("data.txt")


# i am the worst programmer to ever live
def challenge_1(text):
    text = text.split("\n")
    cards = []

    for line in text:
        num_of_wins = 0
        line = line.split(":")[-1]
        winning_numbers = line.split("|")[0].split()
        your_numbers = line.split("|")[1].split()
        # print(winning_numbers)
        # print(your_numbers)
        for i in your_numbers:
            if i in winning_numbers:
                # print(i)
                num_of_wins += 1
        cards.append(num_of_wins)

    # print(cards)
    x = 0
    for i in cards:
        if i == 0:
            continue
        x += int(math.pow(2, i - 1))
    print(f"Challenge 1: {x}")


def challenge_2(text):
    text = text.split("\n")
    cards_data = []
    num_of_cards = []

    for line in text:
        num_of_wins = 0
        line = line.split(":")[-1]
        winning_numbers = line.split("|")[0].split()
        your_numbers = line.split("|")[1].split()
        # print(winning_numbers)
        # print(your_numbers)
        for i in your_numbers:
            if i in winning_numbers:
                # print(i)
                num_of_wins += 1
        if num_of_wins > 0:
            winner = True
        else:
            winner = False
        cards_data.append(num_of_wins)
        num_of_cards.append(1)

    new_num_of_cards = copy.deepcopy(num_of_cards)
    newer_num_of_cards = copy.deepcopy(new_num_of_cards)
    winners_present = True
    while winners_present:
        for i, card in reversed(list(enumerate(new_num_of_cards))):
            # print(cards_data[i] + 1)
            for m in range(1, cards_data[i] + 1):
                if new_num_of_cards[i] <= 0:
                    break
                # print(f"card: {i + 1}, amount={new_num_of_cards[i]}")
                # print(i + m)
                # print(i + m)
                newer_num_of_cards[i + m] += new_num_of_cards[i]
                num_of_cards[i + m] += new_num_of_cards[i]
            newer_num_of_cards[i] = 0
            new_num_of_cards = copy.deepcopy(newer_num_of_cards)
        meow = False
        for i in new_num_of_cards:
            if i > 0:
                meow = True
        if not meow:
            winners_present = False

    x = 0
    for i in num_of_cards:
        x += i
    print(f"Challenge 2: {x}")


long_text = long_text.read()
challenge_1(long_text)
challenge_2(long_text)
