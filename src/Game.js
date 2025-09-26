import Player from './entities/Player.js';
import Nibble from './entities/Nibble.js';
import AngryDog from './entities/AngryDog.js';
import Projectile from './entities/Projectile.js';
import Obstacle from './entities/Obstacle.js';
import Boss from './entities/Boss.js';
import ParticleSystem from './systems/ParticleSystem.js';
import CollisionSystem from './systems/CollisionSystem.js';
import InputSystem from './systems/InputSystem.js';
import AudioSystem from './systems/AudioSystem.js';

/**
 * Main Game class - orchestrates all game systems and entities
 */
class Game {
    constructor() {
        this.canvas = document.getElementById('gameCanvas');
        this.ctx = this.canvas.getContext('2d');
        this.width = this.canvas.width;
        this.height = this.canvas.height;
        
        // Game state
        this.gameState = 'playing'; // 'playing', 'gameOver', 'paused'
        this.score = 0;
        this.distance = 0;
        this.speed = 2;
        this.baseSpeed = 2;
        
        // Initialize game systems
        this.inputSystem = new InputSystem();
        this.collisionSystem = new CollisionSystem();
        this.particleSystem = new ParticleSystem();
        this.audioSystem = new AudioSystem();
        
        // Initialize entities
        this.player = new Player(100, this.height / 2);
        this.nibble = new Nibble(80, this.height / 2 + 20);
        
        // Game object arrays
        this.angryDogs = [];
        this.projectiles = [];
        this.obstacles = [];
        this.powerUps = [];
        this.boss = null;
        
        // Spawn timers
        this.obstacleTimer = 0;
        this.obstacleSpawnRate = 120;
        this.dogTimer = 0;
        this.dogSpawnRate = 300;
        
        // Boss system
        this.bossSpawned = false;
        this.bossSpawnDistance = 5000; // Spawn boss at 5000m
        
        // Performance tracking
        this.lastTime = 0;
        this.fps = 0;
        this.frameCount = 0;
        
        this.setupCollisionHandlers();
        this.setupInputHandlers();
        this.startAudio();
        this.gameLoop();
    }

    /**
     * Set up collision detection handlers
     */
    setupCollisionHandlers() {
        // Projectile vs Obstacles
        this.collisionSystem.registerCollision('projectile', 'obstacle', (projectile, obstacle, projIndex, obsIndex) => {
            const destroyed = obstacle.takeDamage(projectile.getDamage());
            this.projectiles.splice(projIndex, 1);
            this.particleSystem.createExplosion(obstacle.x, obstacle.y, obstacle.color, 8);
            
            if (destroyed) {
                this.obstacles.splice(obsIndex, 1);
                this.score += 100;
                this.createPowerUp(obstacle.x, obstacle.y);
                this.audioSystem.playObstacleDestroyed(obstacle.type);
            }
            
            return { removeObj1: true, removeObj2: destroyed };
        });

        // Projectile vs Angry Dogs
        this.collisionSystem.registerCollision('projectile', 'angryDog', (projectile, dog, projIndex, dogIndex) => {
            const died = dog.takeDamage(projectile.getDamage());
            this.projectiles.splice(projIndex, 1);
            this.particleSystem.createExplosion(dog.x, dog.y, dog.color, 6);
            
            if (died) {
                this.angryDogs.splice(dogIndex, 1);
                this.score += 50;
                this.audioSystem.playExplosion();
            }
            
            return { removeObj1: true, removeObj2: died };
        });

        // Player vs Obstacles
        this.collisionSystem.registerCollision('player', 'obstacle', (player, obstacle, playerIndex, obsIndex) => {
            const died = player.takeDamage(10);
            this.obstacles.splice(obsIndex, 1);
            this.particleSystem.createDamage(player.x, player.y, 10);
            this.audioSystem.playDamage();
            
            if (died) {
                this.gameOver();
            }
            
            return { removeObj1: false, removeObj2: true };
        });

        // Player vs Angry Dogs
        this.collisionSystem.registerCollision('player', 'angryDog', (player, dog, playerIndex, dogIndex) => {
            const died = player.takeDamage(15);
            this.angryDogs.splice(dogIndex, 1);
            this.particleSystem.createDamage(player.x, player.y, 12);
            
            if (died) {
                this.gameOver();
            }
            
            return { removeObj1: false, removeObj2: true };
        });

        // Nibble vs Angry Dogs
        this.collisionSystem.registerCollision('nibble', 'angryDog', (nibble, dog, nibbleIndex, dogIndex) => {
            const died = nibble.takeDamage(20);
            this.angryDogs.splice(dogIndex, 1);
            this.particleSystem.createDamage(nibble.x, nibble.y, 8);
            
            if (died) {
                this.gameOver();
            }
            
            return { removeObj1: false, removeObj2: true };
        });

        // Player vs Power Ups
        this.collisionSystem.registerCollision('player', 'powerUp', (player, powerUp, playerIndex, powerUpIndex) => {
            this.collectPowerUp(powerUp);
            this.powerUps.splice(powerUpIndex, 1);
            this.audioSystem.playPowerUp();
            
            return { removeObj1: false, removeObj2: true };
        });

        // Nibble vs Power Ups (healing)
        this.collisionSystem.registerCollision('nibble', 'powerUp', (nibble, powerUp, nibbleIndex, powerUpIndex) => {
            if (powerUp.type === 'health') {
                nibble.heal(30);
                this.powerUps.splice(powerUpIndex, 1);
                this.particleSystem.createHealing(nibble.x, nibble.y, 6);
            }
            
            return { removeObj1: false, removeObj2: powerUp.type === 'health' };
        });

        // Projectile vs Boss
        this.collisionSystem.registerCollision('projectile', 'boss', (projectile, boss, projIndex, bossIndex) => {
            const defeated = boss.takeDamage(projectile.getDamage());
            this.projectiles.splice(projIndex, 1);
            this.particleSystem.createExplosion(boss.x, boss.y, boss.color, 12);
            
            if (defeated) {
                this.boss = null;
                this.bossSpawned = false;
                this.score += 1000;
                this.audioSystem.playExplosion();
            }
            
            return { removeObj1: true, removeObj2: defeated };
        });

        // Player vs Boss
        this.collisionSystem.registerCollision('player', 'boss', (player, boss, playerIndex, bossIndex) => {
            const died = player.takeDamage(25);
            this.particleSystem.createDamage(player.x, player.y, 15);
            this.audioSystem.playDamage();
            
            if (died) {
                this.gameOver();
            }
            
            return { removeObj1: false, removeObj2: false };
        });
    }

    /**
     * Start audio system
     */
    startAudio() {
        // Resume audio context on first user interaction
        document.addEventListener('click', () => {
            this.audioSystem.resumeAudio();
            this.audioSystem.playBackgroundMusic();
        }, { once: true });
    }

    /**
     * Set up input event handlers
     */
    setupInputHandlers() {
        this.inputSystem.onKeyDown = (e) => {
            if (e.code === 'Space') {
                e.preventDefault();
                this.shoot();
            }
            
            if (e.code === 'KeyR' && this.gameState === 'gameOver') {
                this.restart();
            }
            
            if (e.code === 'KeyP') {
                this.togglePause();
            }
        };

        this.inputSystem.onTouchEnd = (e) => {
            const swipe = this.inputSystem.detectSwipe(e);
            if (swipe) {
                this.handleSwipe(swipe);
            } else if (this.inputSystem.detectTap(e)) {
                this.shoot();
            }
        };
    }

    /**
     * Handle swipe gestures for mobile
     * @param {string} direction - Swipe direction
     */
    handleSwipe(direction) {
        switch (direction) {
            case 'up':
                // Jump - move player up
                this.player.vy = -this.player.speed * 2;
                break;
            case 'down':
                // Slide/Duck - move player down
                this.player.vy = this.player.speed * 2;
                break;
            case 'left':
                // Move left
                this.player.vx = -this.player.speed;
                break;
            case 'right':
                // Move right
                this.player.vx = this.player.speed;
                break;
        }
        
        // Create visual feedback
        this.particleSystem.createTrail(this.player.x, this.player.y, '#00ffff', 3);
    }

    /**
     * Update game state
     * @param {number} deltaTime - Time elapsed since last update
     */
    update(deltaTime) {
        if (this.gameState !== 'playing') return;

        // Update entities
        this.player.update(this.inputSystem.keys, deltaTime);
        this.nibble.update(this.player, deltaTime);

        // Update game objects
        this.updateAngryDogs(deltaTime);
        this.updateProjectiles(deltaTime);
        this.updateObstacles(deltaTime);
        this.updatePowerUps(deltaTime);
        this.updateBoss(deltaTime);

        // Update systems
        this.particleSystem.update(deltaTime);

        // Spawn new objects
        this.spawnObstacles();
        this.spawnAngryDogs();
        this.spawnBoss();

        // Update game progression
        this.distance += this.speed;
        this.score = Math.floor(this.distance / 10);

        // Increase difficulty over time
        this.speed = this.baseSpeed + (this.distance / 1000) * 0.5;

        // Check collisions
        this.checkAllCollisions();
        this.checkBossCollisions();

        // Update UI
        this.updateUI();
    }

    /**
     * Update angry dogs
     * @param {number} deltaTime - Time elapsed since last update
     */
    updateAngryDogs(deltaTime) {
        for (let i = this.angryDogs.length - 1; i >= 0; i--) {
            const dog = this.angryDogs[i];
            dog.update(this.player, this.nibble, deltaTime);

            if (dog.shouldRemove(this.width, this.height)) {
                this.angryDogs.splice(i, 1);
            }
        }
    }

    /**
     * Update projectiles
     * @param {number} deltaTime - Time elapsed since last update
     */
    updateProjectiles(deltaTime) {
        for (let i = this.projectiles.length - 1; i >= 0; i--) {
            const projectile = this.projectiles[i];
            projectile.update(deltaTime);

            if (projectile.isOffScreen(this.width, this.height)) {
                this.projectiles.splice(i, 1);
            }
        }
    }

    /**
     * Update obstacles
     * @param {number} deltaTime - Time elapsed since last update
     */
    updateObstacles(deltaTime) {
        for (let i = this.obstacles.length - 1; i >= 0; i--) {
            const obstacle = this.obstacles[i];
            obstacle.update(deltaTime);
            obstacle.x -= this.speed;

            if (this.collisionSystem.isOffScreen(obstacle, this.width, this.height)) {
                this.obstacles.splice(i, 1);
            }
        }
    }

    /**
     * Update power ups
     * @param {number} deltaTime - Time elapsed since last update
     */
    updatePowerUps(deltaTime) {
        for (let i = this.powerUps.length - 1; i >= 0; i--) {
            const powerUp = this.powerUps[i];
            powerUp.x -= this.speed;
            powerUp.rotation += 0.1;

            if (this.collisionSystem.isOffScreen(powerUp, this.width, this.height)) {
                this.powerUps.splice(i, 1);
            }
        }
    }

    /**
     * Check all collision types
     */
    checkAllCollisions() {
        this.collisionSystem.checkCollisions(this.projectiles, this.obstacles, 'projectile', 'obstacle');
        this.collisionSystem.checkCollisions(this.projectiles, this.angryDogs, 'projectile', 'angryDog');
        this.collisionSystem.checkCollisions([this.player], this.obstacles, 'player', 'obstacle');
        this.collisionSystem.checkCollisions([this.player], this.angryDogs, 'player', 'angryDog');
        this.collisionSystem.checkCollisions([this.nibble], this.angryDogs, 'nibble', 'angryDog');
        this.collisionSystem.checkCollisions([this.player], this.powerUps, 'player', 'powerUp');
        this.collisionSystem.checkCollisions([this.nibble], this.powerUps, 'nibble', 'powerUp');
    }

    /**
     * Check boss-specific collisions
     */
    checkBossCollisions() {
        if (!this.boss) return;

        // Projectile vs Boss
        this.collisionSystem.checkCollisions(this.projectiles, [this.boss], 'projectile', 'boss');
        
        // Player vs Boss
        this.collisionSystem.checkCollisions([this.player], [this.boss], 'player', 'boss');
        
        // Check boss bark bombs vs player
        if (this.boss.barkBombs) {
            this.boss.barkBombs.forEach((bomb, index) => {
                if (this.collisionSystem.checkCollision(this.player, bomb)) {
                    const died = this.player.takeDamage(bomb.damage);
                    this.particleSystem.createExplosion(bomb.x, bomb.y, '#ffff00', 8);
                    this.audioSystem.playDamage();
                    this.boss.barkBombs.splice(index, 1);
                    
                    if (died) {
                        this.gameOver();
                    }
                }
            });
        }
        
        // Check mini chihuahuas vs player
        if (this.boss.miniChihuahuas) {
            this.boss.miniChihuahuas.forEach((mini, index) => {
                if (this.collisionSystem.checkCollision(this.player, mini)) {
                    const died = this.player.takeDamage(10);
                    this.particleSystem.createDamage(this.player.x, this.player.y, 5);
                    this.audioSystem.playDamage();
                    this.boss.miniChihuahuas.splice(index, 1);
                    
                    if (died) {
                        this.gameOver();
                    }
                }
            });
        }
    }

    /**
     * Spawn obstacles
     */
    spawnObstacles() {
        this.obstacleTimer++;
        if (this.obstacleTimer >= this.obstacleSpawnRate) {
            this.obstacleTimer = 0;
            this.createRandomObstacle();
        }
    }

    /**
     * Spawn angry dogs
     */
    spawnAngryDogs() {
        this.dogTimer++;
        if (this.dogTimer >= this.dogSpawnRate) {
            this.dogTimer = 0;
            this.createAngryDog();
        }
    }

    /**
     * Spawn boss when distance threshold is reached
     */
    spawnBoss() {
        if (!this.bossSpawned && this.distance >= this.bossSpawnDistance) {
            this.boss = new Boss(this.width - 150, this.height / 2);
            this.bossSpawned = true;
            this.audioSystem.playExplosion(); // Boss entrance sound
        }
    }

    /**
     * Update boss
     * @param {number} deltaTime - Time elapsed since last update
     */
    updateBoss(deltaTime) {
        if (this.boss) {
            this.boss.update(this.player, this.nibble, deltaTime);
        }
    }

    /**
     * Create a random obstacle
     */
    createRandomObstacle() {
        const obstacleTypes = [
            'bouncyBoneDrone',
            'slobberMine',
            'holoFleaSwarm',
            'runawayDogWalker',
            'kibbleCannon',
            'neonChewToy'
        ];

        const type = obstacleTypes[Math.floor(Math.random() * obstacleTypes.length)];
        const y = Math.random() * (this.height - 100) + 50;

        this.createObstacle(type, this.width + 50, y);
    }

    /**
     * Create an obstacle
     * @param {string} type - Obstacle type
     * @param {number} x - X position
     * @param {number} y - Y position
     */
    createObstacle(type, x, y) {
        const obstacle = new Obstacle(type, x, y);
        this.obstacles.push(obstacle);
    }

    /**
     * Create an angry dog
     */
    createAngryDog() {
        const dog = new AngryDog(this.width + 50, Math.random() * (this.height - 100) + 50);
        this.angryDogs.push(dog);
    }

    /**
     * Shoot a projectile
     */
    shoot() {
        if (this.player.canShoot()) {
            this.player.consumeAmmo();
            
            const projectile = new Projectile(
                this.player.x + this.player.width,
                this.player.y + this.player.height / 2,
                8,
                0
            );
            
            this.projectiles.push(projectile);
            this.particleSystem.createTrail(
                this.player.x + this.player.width,
                this.player.y + this.player.height / 2,
                '#ffff00',
                3
            );
            
            // Play shooting sound
            this.audioSystem.playShoot();
        }
    }

    /**
     * Create a power up
     * @param {number} x - X position
     * @param {number} y - Y position
     */
    createPowerUp(x, y) {
        const powerUpTypes = ['ammo', 'health', 'speed'];
        const type = powerUpTypes[Math.floor(Math.random() * powerUpTypes.length)];

        const powerUp = {
            x: x,
            y: y,
            width: 20,
            height: 20,
            type: type,
            color: type === 'ammo' ? '#ffff00' : type === 'health' ? '#00ff00' : '#ff6b6b',
            rotation: 0,
            isActive: true,
            vx: 0,
            vy: 0
        };

        this.powerUps.push(powerUp);
    }

    /**
     * Collect a power up
     * @param {Object} powerUp - Power up object
     */
    collectPowerUp(powerUp) {
        switch (powerUp.type) {
            case 'ammo':
                this.player.addAmmo(5);
                break;
            case 'health':
                this.player.heal(25);
                break;
            case 'speed':
                this.speed += 0.5;
                setTimeout(() => { this.speed -= 0.5; }, 5000);
                break;
        }

        this.particleSystem.createExplosion(powerUp.x, powerUp.y, powerUp.color, 5);
    }

    /**
     * Update UI elements
     */
    updateUI() {
        document.getElementById('score').textContent = this.score;
        document.getElementById('distance').textContent = Math.floor(this.distance);
        document.getElementById('nibbleHealth').textContent = Math.max(0, Math.floor(this.nibble.health));
        document.getElementById('ammo').textContent = Math.floor(this.player.ammo);
    }

    /**
     * Game over
     */
    gameOver() {
        this.gameState = 'gameOver';
        this.particleSystem.createExplosion(this.player.x, this.player.y, '#ff0000', 20);
        this.audioSystem.playGameOver();
    }

    /**
     * Restart the game
     */
    restart() {
        this.gameState = 'playing';
        this.score = 0;
        this.distance = 0;
        this.speed = this.baseSpeed;

        // Reset player
        this.player.health = 100;
        this.player.ammo = this.player.maxAmmo;
        this.player.x = 100;
        this.player.y = this.height / 2;
        this.player.isActive = true;

        // Reset Nibble
        this.nibble.health = 100;
        this.nibble.x = 80;
        this.nibble.y = this.height / 2 + 20;
        this.nibble.isFetching = false;
        this.nibble.isActive = true;

        // Clear all objects
        this.obstacles = [];
        this.angryDogs = [];
        this.projectiles = [];
        this.powerUps = [];
        this.boss = null;
        this.bossSpawned = false;
        this.particleSystem.clear();

        // Reset timers
        this.obstacleTimer = 0;
        this.dogTimer = 0;
    }

    /**
     * Toggle pause state
     */
    togglePause() {
        if (this.gameState === 'playing') {
            this.gameState = 'paused';
        } else if (this.gameState === 'paused') {
            this.gameState = 'playing';
        }
    }

    /**
     * Render the game
     * @param {number} deltaTime - Time elapsed since last render
     */
    render(deltaTime) {
        // Clear canvas with trail effect
        this.ctx.fillStyle = 'rgba(0, 4, 40, 0.1)';
        this.ctx.fillRect(0, 0, this.width, this.height);

        // Draw background
        this.drawBackground();

        // Draw game objects
        this.drawObstacles();
        this.drawPowerUps();
        this.drawAngryDogs();
        this.drawProjectiles();
        this.drawBoss();
        this.particleSystem.render(this.ctx);

        // Draw player and Nibble
        this.player.render(this.ctx);
        this.nibble.render(this.ctx);

        // Draw UI overlays
        if (this.gameState === 'gameOver') {
            this.drawGameOver();
        } else if (this.gameState === 'paused') {
            this.drawPauseScreen();
        }

        // Draw debug info
        this.drawDebugInfo();
    }

    /**
     * Draw background elements
     */
    drawBackground() {
        // Draw cyberpunk city skyline with neon glow
        this.ctx.shadowColor = '#00ffff';
        this.ctx.shadowBlur = 10;
        this.ctx.fillStyle = 'rgba(0, 255, 255, 0.3)';
        for (let i = 0; i < 20; i++) {
            const x = (i * 100 - this.distance * 0.1) % (this.width + 100);
            const height = 50 + Math.sin(i + this.distance * 0.01) * 30;
            this.ctx.fillRect(x, this.height - height, 80, height);
        }
        this.ctx.shadowBlur = 0;

        // Draw neon grid with pulsing effect
        const pulse = Math.sin(this.distance * 0.01) * 0.1 + 0.2;
        this.ctx.strokeStyle = `rgba(0, 255, 255, ${pulse})`;
        this.ctx.lineWidth = 1;
        this.ctx.shadowColor = '#00ffff';
        this.ctx.shadowBlur = 2;
        
        for (let i = 0; i < this.width; i += 50) {
            this.ctx.beginPath();
            this.ctx.moveTo(i, 0);
            this.ctx.lineTo(i, this.height);
            this.ctx.stroke();
        }
        for (let i = 0; i < this.height; i += 50) {
            this.ctx.beginPath();
            this.ctx.moveTo(0, i);
            this.ctx.lineTo(this.width, i);
            this.ctx.stroke();
        }
        this.ctx.shadowBlur = 0;

        // Draw floating particles
        this.ctx.fillStyle = 'rgba(255, 0, 255, 0.3)';
        for (let i = 0; i < 10; i++) {
            const x = (i * 80 + this.distance * 0.05) % (this.width + 20);
            const y = 50 + Math.sin(i + this.distance * 0.02) * 100;
            this.ctx.fillRect(x, y, 2, 2);
        }

        // Draw data streams
        this.ctx.strokeStyle = 'rgba(0, 255, 0, 0.4)';
        this.ctx.lineWidth = 2;
        for (let i = 0; i < 5; i++) {
            const x = (i * 160 + this.distance * 0.2) % (this.width + 40);
            this.ctx.beginPath();
            this.ctx.moveTo(x, 0);
            this.ctx.lineTo(x + 20, this.height);
            this.ctx.stroke();
        }
    }

    /**
     * Draw obstacles
     */
    drawObstacles() {
        this.obstacles.forEach(obstacle => {
            obstacle.render(this.ctx);
        });
    }

    /**
     * Draw power ups
     */
    drawPowerUps() {
        this.powerUps.forEach(powerUp => {
            if (!powerUp.isActive) return;
            
            this.ctx.save();
            this.ctx.translate(powerUp.x + powerUp.width / 2, powerUp.y + powerUp.height / 2);
            this.ctx.rotate(powerUp.rotation);
            this.ctx.fillStyle = powerUp.color;
            this.ctx.fillRect(-powerUp.width / 2, -powerUp.height / 2, powerUp.width, powerUp.height);
            this.ctx.restore();
        });
    }

    /**
     * Draw angry dogs
     */
    drawAngryDogs() {
        this.angryDogs.forEach(dog => {
            dog.render(this.ctx);
        });
    }

    /**
     * Draw projectiles
     */
    drawProjectiles() {
        this.projectiles.forEach(projectile => {
            projectile.render(this.ctx);
        });
    }

    /**
     * Draw boss
     */
    drawBoss() {
        if (this.boss) {
            this.boss.render(this.ctx);
        }
    }

    /**
     * Draw game over screen
     */
    drawGameOver() {
        this.ctx.fillStyle = 'rgba(0, 0, 0, 0.8)';
        this.ctx.fillRect(0, 0, this.width, this.height);

        this.ctx.fillStyle = '#ff0000';
        this.ctx.font = '48px Arial';
        this.ctx.textAlign = 'center';
        this.ctx.fillText('GAME OVER', this.width / 2, this.height / 2 - 50);

        this.ctx.fillStyle = '#ffffff';
        this.ctx.font = '24px Arial';
        this.ctx.fillText(`Final Score: ${this.score}`, this.width / 2, this.height / 2);
        this.ctx.fillText('Press R to Restart', this.width / 2, this.height / 2 + 50);

        this.ctx.textAlign = 'left';
    }

    /**
     * Draw pause screen
     */
    drawPauseScreen() {
        this.ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
        this.ctx.fillRect(0, 0, this.width, this.height);

        this.ctx.fillStyle = '#ffff00';
        this.ctx.font = '48px Arial';
        this.ctx.textAlign = 'center';
        this.ctx.fillText('PAUSED', this.width / 2, this.height / 2);

        this.ctx.fillStyle = '#ffffff';
        this.ctx.font = '24px Arial';
        this.ctx.fillText('Press P to Resume', this.width / 2, this.height / 2 + 50);

        this.ctx.textAlign = 'left';
    }

    /**
     * Draw debug information
     */
    drawDebugInfo() {
        if (this.gameState !== 'playing') return;

        this.ctx.fillStyle = '#ffffff';
        this.ctx.font = '12px Arial';
        this.ctx.fillText(`FPS: ${this.fps}`, 10, this.height - 60);
        this.ctx.fillText(`Particles: ${this.particleSystem.getParticleCount()}`, 10, this.height - 45);
        this.ctx.fillText(`Dogs: ${this.angryDogs.length}`, 10, this.height - 30);
        this.ctx.fillText(`Obstacles: ${this.obstacles.length}`, 10, this.height - 15);
    }

    /**
     * Main game loop
     * @param {number} currentTime - Current timestamp
     */
    gameLoop(currentTime = 0) {
        const deltaTime = currentTime - this.lastTime;
        this.lastTime = currentTime;

        // Calculate FPS
        this.frameCount++;
        if (this.frameCount % 60 === 0) {
            this.fps = Math.round(1000 / deltaTime);
        }

        this.update(deltaTime);
        this.render(deltaTime);

        requestAnimationFrame((time) => this.gameLoop(time));
    }
}

export default Game;