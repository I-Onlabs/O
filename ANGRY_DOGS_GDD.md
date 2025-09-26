# Angry Dogs - Game Design Document

## Game Overview

**Title:** Angry Dogs  
**Genre:** Endless Runner/Shooter Hybrid  
**Platform:** Mobile (iOS/Android) + PC  
**Target Audience:** Casual to Mid-core gamers, ages 13+  
**Development Time:** 6-8 months  

### Core Concept
In 2149 Neo-Tokyo, you're Riley, a clumsy hacker who accidentally rewrote the AI of corporate guard dogs, turning them into "Angry Dogs 2.0"—a horde of cybernetic hounds with glowing eyes, hydraulic jaws, and a grudge. They're chasing you through a neon-lit megacity, and you must sprint, shoot obstacles to clear paths, and protect your glitchy pup, Nibble, who's the only dog still on your side.

### Tone & Style
- **Visual:** Cyberpunk meets cartoon chaos - neon-drenched city with cartoonish effects
- **Humor:** Silly and chaotic, like Plants vs. Zombies meets Cyberpunk 2077
- **Audio:** Synthwave soundtrack with remixed dog barks and comical sound effects

## Core Mechanics

### Movement System
- **Auto-Forward Sprint:** Continuous forward movement in 3D third-person view
- **Controls:** 
  - Swipe up: Jump
  - Swipe down: Slide/Duck
  - Swipe left/right: Lane switching
  - Tap: Shoot forward
- **Speed Management:** Slowing down lets Angry Dogs close in for attacks

### Combat System
- **Forward-Firing Gadgets:** Riley wields hackable tech (zap pistol, glitch grenade)
- **Ammo System:** Regenerates over time, missed shots cost time
- **Obstacle Interaction:** Shooting obstacles clears paths and creates defenses/boosts

### Nibble's Role
- **Companion AI:** Runs alongside Riley, vulnerable to attacks
- **Abilities:**
  - Fetch KibbleCoins and power-ups
  - Howl ability that slows hounds
  - Dig for hidden items
  - Trigger special abilities when healthy
- **Health System:** Takes damage from stray attacks, reducing buffs until healed

### Progression System
- **Score:** Based on distance traveled, obstacles cleared, and Nibble saves
- **Checkpoints:** Survival milestones with upgrades
- **Boss Fights:** Giant cyber-dogs with unique mechanics
- **Daily Challenges:** "Dog Park Dash" and other themed runs
- **Leaderboards:** Global and friend-based competition

## Obstacle System

### 1. Bouncy Bone Drones (Aerial Annoyance)
- **Description:** Drones carrying oversized holographic bones that bounce wildly
- **Threat:** Knock Riley back with cartoonish SPROING effect
- **Interaction:** Shoot to shatter into "holo-treats" that lure hounds into pile-ups
- **Reward:** "Tasty Turbo" speed boost, "Bone Boomerang" for Nibble

### 2. Slobber Mines (Sticky Traps)
- **Description:** Corporate traps spitting neon goo that slows movement
- **Threat:** Sticky feet, reduced speed while hounds bark mockingly
- **Interaction:** Blast to create "Slobber Slick" that makes hounds slip and slide
- **Reward:** Speed boost from skating on slick, "Goo Bubble" shield for Nibble

### 3. Holo-Flea Swarms (Itchy Infestation)
- **Description:** Clouds of robotic fleas causing "Itchy Debuff"
- **Threat:** Screen shakes, controls wobble, blocks paths
- **Interaction:** EMP blast fries fleas into "spark sprinkles" that stun hounds
- **Reward:** "Flea Flicker" bouncing shots, "Flea-Proof" coating for Nibble

### 4. Runaway Dog Walkers (Leash Lunacy)
- **Description:** Malfunctioning dog-walker bots with electrified leashes
- **Threat:** Tripping Riley with whip-like leash attacks
- **Interaction:** Shoot to tangle leashes into "Shock Net" traps
- **Reward:** Nets as slingshots for vaulting, "Leash Shield" for Nibble

### 5. Kibble Cannon Turrets (Treat Barrage)
- **Description:** Automated turrets firing high-speed kibble pellets
- **Threat:** Slows Riley, draws hounds into feeding frenzy
- **Interaction:** Overload causes "Kibble Explosion" scattering treats
- **Reward:** "Munchie Missile" path-clearing weapon, "Treat Trail" for Nibble

### 6. Neon Chew Toy Piles (Bouncy Blockades)
- **Description:** Heaps of glowing chew toys blocking lanes
- **Threat:** Bouncing erratically, hounds chew and spit rubber bits
- **Interaction:** Pop into "Squeaky Storm" that stuns hounds
- **Reward:** Ramp formation for jumps, "Squeaky Decoys" for Nibble

### 7. Boss: Cyber-Chihuahua King (Tiny Terror)
- **Description:** Pint-sized chihuahua in massive mech-suit
- **Threat:** "Bark Bombs" that crater ground, spawns mini-chihuahuas
- **Interaction:** Target speakers for "Yap Overload" that deafens hounds
- **Reward:** "Bark Blaster" super-weapon, "Chihuahua Charm" for Nibble

## Technical Specifications

### Platform Requirements
- **Mobile:** iOS 12+, Android 8.0+
- **PC:** Windows 10+, macOS 10.14+, Linux
- **Performance:** 60 FPS target, scalable graphics settings

### Art Style
- **3D Low-Poly:** Stylized cyberpunk aesthetic
- **Color Palette:** Neon blues, purples, pinks with high contrast
- **Effects:** Particle systems for explosions, trails, and environmental effects
- **UI:** Futuristic HUD with glitch effects

### Audio Design
- **Music:** Synthwave/cyberpunk soundtrack with dynamic intensity
- **SFX:** Cartoonish sound effects for all interactions
- **Voice:** Riley's sarcastic quips, Nibble's malfunctioning yips
- **Ambient:** City sounds, distant barking, electronic hums

## Monetization Strategy

### Free-to-Play Model
- **Core Game:** Free with ads between runs
- **Premium Pass:** $4.99/month for ad-free experience + exclusive content
- **Cosmetics:** Riley outfits, Nibble accessories, weapon skins
- **Power-ups:** Temporary boosts purchasable with in-game currency

### In-Game Currency
- **KibbleCoins:** Earned through gameplay, used for upgrades
- **Cyber-Credits:** Premium currency for cosmetics and convenience
- **Daily Rewards:** Login bonuses and challenge completions

## Development Phases

### Phase 1: Core Prototype (2 months)
- Basic movement and shooting mechanics
- Simple obstacle system
- Nibble AI and interaction
- Basic Angry Dogs chase behavior

### Phase 2: Content & Polish (2 months)
- All obstacle types implemented
- Boss fight system
- UI/UX implementation
- Audio integration

### Phase 3: Advanced Features (2 months)
- Progression system
- Daily challenges
- Multiplayer/leaderboards
- Monetization integration

### Phase 4: Platform Optimization (2 months)
- Mobile optimization
- Platform-specific features
- Performance tuning
- Launch preparation

## Risk Mitigation

### Technical Risks
- **Performance:** Mobile optimization challenges
- **Mitigation:** Scalable graphics, efficient asset management

### Design Risks
- **Difficulty Curve:** Balancing challenge vs. accessibility
- **Mitigation:** Extensive playtesting, adaptive difficulty

### Market Risks
- **Competition:** Saturated endless runner market
- **Mitigation:** Unique cyberpunk theme, strong character appeal

## Success Metrics

### Engagement
- **Retention:** 40% Day 1, 20% Day 7, 10% Day 30
- **Session Length:** Average 3-5 minutes per run
- **Daily Active Users:** Target 100K+ within 3 months

### Monetization
- **ARPU:** $2-3 per paying user
- **Conversion Rate:** 3-5% free-to-paid
- **LTV:** $15-25 per paying user

## Conclusion

Angry Dogs combines the addictive nature of endless runners with the humor and heart of a buddy comedy. The cyberpunk setting provides visual flair while the dog-centric obstacles create memorable, shareable moments. The core loop of "run, shoot, protect Nibble" is simple to learn but offers depth through obstacle interaction and strategic decision-making.

The game's success will depend on tight controls, satisfying feedback, and the emotional connection players form with Nibble. By focusing on polish and player experience, Angry Dogs can carve out its own space in the competitive mobile gaming market.