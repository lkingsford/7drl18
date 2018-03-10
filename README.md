# The Dark Count
## Lachlan Kingsford and Friends

This is my entry to the 2018 7drl competition. The compiled version is
available on [itch.io](https://nerdygentleman.itch.io/7drl18). You can also
follow development on [twitter](https://twitter.com/thelochok).

The game is an attempt to take Batman Arkham Asylum/Arkham City/Shadows of 
Mordor style combat into the turn based space.

# Special Note (For other programmers)

This game was hacked together in 7 days. It's not indicative of my normal 
style - it's instead very rushed, sometimes using bad algorithms or techniques,
sometimes with code that I would reject out of hand in code review due to
bad variable names, comments, code grammar etc. Please don't judge me too
harshly.

# Rules (so far)

## Implemented combat rules
- If enemy within 4 spaces, attack them
- If attack enemy:
	- Stun the enemy a turn
	- Take their space pushing them away
	- If can't take their space, stun them additional turn
	- Damage = momentum
- Each attack adds momentum
- Each turn without attack removes momentum
- Each time attacked remove momentum
- Get attacked pushes away
- If pushed into wall, additional damage
- May spend 1 momentum to move away from attack direction
- May spend X momentum to parry (X = amount of attacks)

- Brutes don't get stunned
- Brutes push back further
- Knifes don't push back
- Knifes can't parry

## Other rules
- If no enemies in sight, move freely
