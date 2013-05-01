amt-examples
============
Enemy Code

Enemy.cs - Serves at the glue holding the AI, Motor, Cargo and Hull of the enemy ship together.  This class contains several static functions that serve to provide information from the list of registered enemies, such as the closest Enemy to a given location.

EnemyMotor.cs - Is responsible for moving the Enemy in fluid manner along the XY plane.  Contains functionality to produce imperfect paths toward a target by setting targetOffsetToDistanceRatio (higher value produces more wobbly movement).

EnemyAI.cs - serves as the base class for AI implementations.  States, in the form of a string, delegate method Dictionary, are set up and changed by the subclass.  This class also includes object avoidance, determined by providing a limited area of sight in the form of a sphere along with a threshold angle.  Upon noticing a potential collision with an object in sight, the subclass is notified and is responsible for reacting accordingly.

Enemies/Spawner/EnemySpawner.cs - is the main enemy spawning mechanism.  Given a series of properties, designers create lists of portals and the conditions under which the next portal in the list can spawn.  Portals, once created, spawn the given enemy set at a given rate.

Player/Player Weapons/Turrets/Turret.cs - is the class responsible for aiming and firing the assigned weapon and controlling the TurretMotor for movement.  Given a relative position, Turrets will attempt to maintain that position with variances introduced for effect.  Turrets in manual firing mode will fire in their configured direction relateive to the given firing angle.  Turrets set to AI mode will pick the best enemy for a given criterea set from the enemies in range and fire at it.  Slow projectile weapons will predict the point of impact and lead their shots accordingly.

Player/Player Weapons/Weapons/ - contains the three types of weapons used in the game, derived from two base classes: projectiles and beams.  Beams can be configured to fire constantly or with on/off variations.  All weapons include the ability to fire at an inconsistent rate.

Player/Player Weapons/Shots/ contains the various types of weapon shots used in the game.  Beams include optional penetration values to determine how many Enemies they can hit before stopping.  Fusion Missiles include Enemy seeking capabilities.