import math
from PyAstronomy import pyasl

# gives electron orbital diagram information for a given element
# i mmmade this years ago and i forgot everything i learned in chemistry
# so idk what this really does anymore sorry
# requires pyastronomy
# > pip install PyAstronomy

def main(atom):
  levels = [
    "1s",
    "2s",
    "2p",
    "3s",
    "3p",
    "4s",
    "3d",
    "4p",
    "5s",
    "4d",
    "5p",
    "6s",
    "4f",
    "5d",
    "6p",
    "7s",
    "5f",
    "6d",
    "7p"
  ]
  
  electron_holds = {
    "s": 2,
    "p": 6,
    "d": 10,
    "f": 14
  }
  
  sublevel_id_conversion = {
    "s": 0,
    "p": 1,
    "d": 2,
    "f": 3
  }
  
  # atom = 33
  
  orbital_filling_diagram = []
  
  electrons = atom 
  current_level = 0
  current_levels_electrons = 0
  while electrons > 0:
    # print(electron_holds[levels[current_level][-1:]])
    if current_levels_electrons >= electron_holds[levels[current_level][-1:]]:
      orbital_filling_diagram.append(current_levels_electrons)
      current_level += 1
      current_levels_electrons = 0
    else:
      current_levels_electrons += 1
      electrons -= 1
  
  orbital_filling_diagram.append(current_levels_electrons)
  
  human_readable_ofd_str = ""
  
  for i, value in enumerate(orbital_filling_diagram):
    
    human_readable_ofd_str += f"{levels[i]}({value}) "
    
  last_electron_thing = human_readable_ofd_str.split()[-1]
  sublevel = last_electron_thing[1]
  energy_level = last_electron_thing[0]
  electron_fill = int(last_electron_thing.split("(")[-1][:-1])
  if electron_fill > electron_holds[sublevel] / 2:
    electron_spin = -0.5
  else:
    electron_spin = 0.5
  
  match sublevel:
    case "s":
      electron_orbital = 0
    case "p":
      # print(electron_fill)
      if electron_fill == 6:
        electron_orbital = 1
      else:
        electron_orbital = math.ceil(electron_fill / 2) % 3 - 1
    case "d":
      if electron_fill == 10:
        electron_orbital = 1
      else:
        electron_orbital = math.ceil(electron_fill / 2) % 5 - 3
    case "f":
      if electron_fill == 14:
        electron_orbital = 1
      else:
        electron_orbital = math.ceil(electron_fill / 2) % 7 - 5
  an = pyasl.AtomicNo()
  print(an.getElementName(atom))
  print(human_readable_ofd_str)
  print()
  print(f"sublevel         l={sublevel_id_conversion[sublevel]}\n"
        f"energy level     n={energy_level}\n"
        f"electron orbital ml={electron_orbital}\n"
        f"electron spin    ms={electron_spin}")
  


if __name__ == "__main__":
  _atom = input("Enter the atomic number of the atom or q to quit: ")
  while _atom.lower().strip() != 'q':
    if _atom.isnumeric():
      main(int(_atom))
    _atom = input("Enter the atomic number of the atom or q to quit: ")
    print("\n\n")
  
