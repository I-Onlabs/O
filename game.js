// Angry Dogs - Cyberpunk Endless Runner
// A prototype implementation of the game concept

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
        
        // Player (Riley)
        this.player = {
            x: 100,
            y: this.height / 2,
            width: 40,
            height: 60,
            vx: 0,
            vy: 0,
            health: 100,
            ammo: 10,
            maxAmmo: 10,
            ammoRegen: 0.02,
            color: '#00ffff'
        };
        
        // Nibble (the good dog)
        this.nibble = {
            x: 80,
            y: this.height / 2 + 20,
            width: 25,
            height: 25,
            health: 100,
            maxHealth: 100,
            color: '#ff6b6b',
            isFetching: false,
            fetchTarget: null,
            fetchProgress: 0
        };
        
        // Game objects
        this.obstacles = [];
        this.angryDogs = [];
        this.projectiles = [];
        this.particles = [];
        this.powerUps = [];
        
        // Obstacle spawn timing
        this.obstacleTimer = 0;
        this.obstacleSpawnRate = 120; // frames
        
        // Angry Dogs spawn timing
        this.dogTimer = 0;
        this.dogSpawnRate = 300; // frames
        
        // Input handling
        this.keys = {};
        this.setupInput();
        
        // Start game loop
        this.gameLoop();
    }
    
    setupInput() {
        document.addEventListener('keydown', (e) => {
            this.keys[e.code] = true;
            
            if (e.code === 'Space') {
                e.preventDefault();
                this.shoot();
            }
            
            if (e.code === 'KeyR' && this.gameState === 'gameOver') {
                this.restart();
            }
        });
        
        document.addEventListener('keyup', (e) => {
            this.keys[e.code] = false;
        });
    }
    
    update() {
        if (this.gameState !== 'playing') return;
        
        // Update player movement
        this.updatePlayer();
        
        // Update Nibble
        this.updateNibble();
        
        // Update game objects
        this.updateObstacles();
        this.updateAngryDogs();
        this.updateProjectiles();
        this.updateParticles();
        this.updatePowerUps();
        
        // Spawn new objects
        this.spawnObstacles();
        this.spawnAngryDogs();
        
        // Update game progression
        this.distance += this.speed;
        this.score = Math.floor(this.distance / 10);
        
        // Increase difficulty over time
        this.speed = this.baseSpeed + (this.distance / 1000) * 0.5;
        
        // Check collisions
        this.checkCollisions();
        
        // Update UI
        this.updateUI();
    }
    
    updatePlayer() {
        // Handle movement
        if (this.keys['ArrowUp'] || this.keys['KeyW']) {
            this.player.vy = -3;
        } else if (this.keys['ArrowDown'] || this.keys['KeyS']) {
            this.player.vy = 3;
        } else {
            this.player.vy *= 0.8; // Damping
        }
        
        if (this.keys['ArrowLeft'] || this.keys['KeyA']) {
            this.player.vx = -2;
        } else if (this.keys['ArrowRight'] || this.keys['KeyD']) {
            this.player.vx = 2;
        } else {
            this.player.vx *= 0.8; // Damping
        }
        
        // Update position
        this.player.x += this.player.vx;
        this.player.y += this.player.vy;
        
        // Keep player in bounds
        this.player.x = Math.max(50, Math.min(this.width - 50, this.player.x));
        this.player.y = Math.max(50, Math.min(this.height - 50, this.player.y));
        
        // Regenerate ammo
        if (this.player.ammo < this.player.maxAmmo) {
            this.player.ammo += this.player.ammoRegen;
            if (this.player.ammo > this.player.maxAmmo) {
                this.player.ammo = this.player.maxAmmo;
            }
        }
    }
    
    updateNibble() {
        // Nibble follows Riley with some lag
        const targetX = this.player.x - 20;
        const targetY = this.player.y + 20;
        
        this.nibble.x += (targetX - this.nibble.x) * 0.1;
        this.nibble.y += (targetY - this.nibble.y) * 0.1;
        
        // If Nibble is fetching, handle fetch behavior
        if (this.nibble.isFetching && this.nibble.fetchTarget) {
            const dx = this.nibble.fetchTarget.x - this.nibble.x;
            const dy = this.nibble.fetchTarget.y - this.nibble.y;
            const distance = Math.sqrt(dx * dx + dy * dy);
            
            if (distance < 30) {
                // Nibble reached the target
                this.nibble.isFetching = false;
                this.nibble.fetchTarget = null;
                this.nibble.health = Math.min(this.nibble.maxHealth, this.nibble.health + 20);
                this.addParticles(this.nibble.x, this.nibble.y, '#00ff00', 5);
            } else {
                // Move towards target
                this.nibble.x += dx * 0.05;
                this.nibble.y += dy * 0.05;
            }
        }
    }
    
    updateObstacles() {
        for (let i = this.obstacles.length - 1; i >= 0; i--) {
            const obstacle = this.obstacles[i];
            obstacle.x -= this.speed;
            
            // Remove obstacles that are off screen
            if (obstacle.x + obstacle.width < 0) {
                this.obstacles.splice(i, 1);
            }
        }
    }
    
    updateAngryDogs() {
        for (let i = this.angryDogs.length - 1; i >= 0; i--) {
            const dog = this.angryDogs[i];
            
            // Move towards player
            const dx = this.player.x - dog.x;
            const dy = this.player.y - dog.y;
            const distance = Math.sqrt(dx * dx + dy * dy);
            
            if (distance > 0) {
                dog.x += (dx / distance) * dog.speed;
                dog.y += (dy / distance) * dog.speed;
            }
            
            // Remove dogs that are off screen or too far behind
            if (dog.x < -50 || dog.x > this.width + 50) {
                this.angryDogs.splice(i, 1);
            }
        }
    }
    
    updateProjectiles() {
        for (let i = this.projectiles.length - 1; i >= 0; i--) {
            const projectile = this.projectiles[i];
            projectile.x += projectile.vx;
            projectile.y += projectile.vy;
            
            // Remove projectiles that are off screen
            if (projectile.x > this.width || projectile.x < 0 || projectile.y < 0 || projectile.y > this.height) {
                this.projectiles.splice(i, 1);
            }
        }
    }
    
    updateParticles() {
        for (let i = this.particles.length - 1; i >= 0; i--) {
            const particle = this.particles[i];
            particle.x += particle.vx;
            particle.y += particle.vy;
            particle.life--;
            particle.alpha = particle.life / particle.maxLife;
            
            if (particle.life <= 0) {
                this.particles.splice(i, 1);
            }
        }
    }
    
    updatePowerUps() {
        for (let i = this.powerUps.length - 1; i >= 0; i--) {
            const powerUp = this.powerUps[i];
            powerUp.x -= this.speed;
            powerUp.rotation += 0.1;
            
            if (powerUp.x + powerUp.width < 0) {
                this.powerUps.splice(i, 1);
            }
        }
    }
    
    spawnObstacles() {
        this.obstacleTimer++;
        if (this.obstacleTimer >= this.obstacleSpawnRate) {
            this.obstacleTimer = 0;
            this.createRandomObstacle();
        }
    }
    
    spawnAngryDogs() {
        this.dogTimer++;
        if (this.dogTimer >= this.dogSpawnRate) {
            this.dogTimer = 0;
            this.createAngryDog();
        }
    }
    
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
    
    createObstacle(type, x, y) {
        const obstacle = {
            type: type,
            x: x,
            y: y,
            width: 60,
            height: 60,
            health: 3,
            maxHealth: 3,
            color: this.getObstacleColor(type),
            effect: null
        };
        
        this.obstacles.push(obstacle);
    }
    
    getObstacleColor(type) {
        const colors = {
            'bouncyBoneDrone': '#ff6b6b',
            'slobberMine': '#4ecdc4',
            'holoFleaSwarm': '#45b7d1',
            'runawayDogWalker': '#96ceb4',
            'kibbleCannon': '#feca57',
            'neonChewToy': '#ff9ff3'
        };
        return colors[type] || '#ffffff';
    }
    
    createAngryDog() {
        const dog = {
            x: this.width + 50,
            y: Math.random() * (this.height - 100) + 50,
            width: 40,
            height: 40,
            speed: 1.5 + Math.random() * 0.5,
            health: 2,
            color: '#ff4757',
            lastAttack: 0
        };
        
        this.angryDogs.push(dog);
    }
    
    shoot() {
        if (this.player.ammo >= 1) {
            this.player.ammo -= 1;
            
            const projectile = {
                x: this.player.x + this.player.width,
                y: this.player.y + this.player.height / 2,
                vx: 8,
            vy: 0,
            width: 8,
            height: 4,
            color: '#ffff00'
        };
        
        this.projectiles.push(projectile);
        this.addParticles(this.player.x + this.player.width, this.player.y + this.player.height / 2, '#ffff00', 3);
    }
}

checkCollisions() {
    // Projectile vs Obstacles
    for (let i = this.projectiles.length - 1; i >= 0; i--) {
        const projectile = this.projectiles[i];
        
        for (let j = this.obstacles.length - 1; j >= 0; j--) {
            const obstacle = this.obstacles[j];
            
            if (this.checkCollision(projectile, obstacle)) {
                // Hit obstacle
                obstacle.health--;
                this.projectiles.splice(i, 1);
                this.addParticles(obstacle.x, obstacle.y, obstacle.color, 8);
                
                if (obstacle.health <= 0) {
                    this.obstacles.splice(j, 1);
                    this.score += 100;
                    this.createPowerUp(obstacle.x, obstacle.y);
                }
                break;
            }
        }
    }
    
    // Projectile vs Angry Dogs
    for (let i = this.projectiles.length - 1; i >= 0; i--) {
        const projectile = this.projectiles[i];
        
        for (let j = this.angryDogs.length - 1; j >= 0; j--) {
            const dog = this.angryDogs[j];
            
            if (this.checkCollision(projectile, dog)) {
                // Hit dog
                dog.health--;
                this.projectiles.splice(i, 1);
                this.addParticles(dog.x, dog.y, dog.color, 6);
                
                if (dog.health <= 0) {
                    this.angryDogs.splice(j, 1);
                    this.score += 50;
                }
                break;
            }
        }
    }
    
    // Player vs Obstacles
    for (let i = this.obstacles.length - 1; i >= 0; i--) {
        const obstacle = this.obstacles[i];
        
        if (this.checkCollision(this.player, obstacle)) {
            // Player hit obstacle
            this.player.health -= 10;
            this.obstacles.splice(i, 1);
            this.addParticles(this.player.x, this.player.y, '#ff0000', 10);
            
            if (this.player.health <= 0) {
                this.gameOver();
            }
        }
    }
    
    // Player vs Angry Dogs
    for (let i = this.angryDogs.length - 1; i >= 0; i--) {
        const dog = this.angryDogs[i];
        
        if (this.checkCollision(this.player, dog)) {
            // Player hit by dog
            this.player.health -= 15;
            this.angryDogs.splice(i, 1);
            this.addParticles(this.player.x, this.player.y, '#ff0000', 12);
            
            if (this.player.health <= 0) {
                this.gameOver();
            }
        }
    }
    
    // Nibble vs Angry Dogs
    for (let i = this.angryDogs.length - 1; i >= 0; i--) {
        const dog = this.angryDogs[i];
        
        if (this.checkCollision(this.nibble, dog)) {
            // Nibble hit by dog
            this.nibble.health -= 20;
            this.angryDogs.splice(i, 1);
            this.addParticles(this.nibble.x, this.nibble.y, '#ff6b6b', 8);
            
            if (this.nibble.health <= 0) {
                this.nibble.health = 0;
                // Game over if Nibble dies
                this.gameOver();
            }
        }
    }
    
    // Player vs Power Ups
    for (let i = this.powerUps.length - 1; i >= 0; i--) {
        const powerUp = this.powerUps[i];
        
        if (this.checkCollision(this.player, powerUp)) {
            this.collectPowerUp(powerUp);
            this.powerUps.splice(i, 1);
        }
    }
    
    // Nibble vs Power Ups (healing)
    for (let i = this.powerUps.length - 1; i >= 0; i--) {
        const powerUp = this.powerUps[i];
        
        if (this.checkCollision(this.nibble, powerUp) && powerUp.type === 'health') {
            this.nibble.health = Math.min(this.nibble.maxHealth, this.nibble.health + 30);
            this.powerUps.splice(i, 1);
            this.addParticles(this.nibble.x, this.nibble.y, '#00ff00', 6);
        }
    }
}

checkCollision(obj1, obj2) {
    return obj1.x < obj2.x + obj2.width &&
           obj1.x + obj1.width > obj2.x &&
           obj1.y < obj2.y + obj2.height &&
           obj1.y + obj1.height > obj2.y;
}

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
        rotation: 0
    };
    
    this.powerUps.push(powerUp);
}

collectPowerUp(powerUp) {
    switch (powerUp.type) {
        case 'ammo':
            this.player.ammo = Math.min(this.player.maxAmmo, this.player.ammo + 5);
            break;
        case 'health':
            this.player.health = Math.min(100, this.player.health + 25);
            break;
        case 'speed':
            this.speed += 0.5;
            setTimeout(() => { this.speed -= 0.5; }, 5000);
            break;
    }
    
    this.addParticles(powerUp.x, powerUp.y, powerUp.color, 5);
}

addParticles(x, y, color, count) {
    for (let i = 0; i < count; i++) {
        const particle = {
            x: x,
            y: y,
            vx: (Math.random() - 0.5) * 4,
            vy: (Math.random() - 0.5) * 4,
            color: color,
            life: 30,
            maxLife: 30,
            alpha: 1
        };
        
        this.particles.push(particle);
    }
}

updateUI() {
    document.getElementById('score').textContent = this.score;
    document.getElementById('distance').textContent = Math.floor(this.distance);
    document.getElementById('nibbleHealth').textContent = Math.max(0, Math.floor(this.nibble.health));
    document.getElementById('ammo').textContent = Math.floor(this.player.ammo);
}

gameOver() {
    this.gameState = 'gameOver';
    this.addParticles(this.player.x, this.player.y, '#ff0000', 20);
}

restart() {
    this.gameState = 'playing';
    this.score = 0;
    this.distance = 0;
    this.speed = this.baseSpeed;
    
    this.player.health = 100;
    this.player.ammo = this.player.maxAmmo;
    this.player.x = 100;
    this.player.y = this.height / 2;
    
    this.nibble.health = 100;
    this.nibble.x = 80;
    this.nibble.y = this.height / 2 + 20;
    this.nibble.isFetching = false;
    
    this.obstacles = [];
    this.angryDogs = [];
    this.projectiles = [];
    this.particles = [];
    this.powerUps = [];
    
    this.obstacleTimer = 0;
    this.dogTimer = 0;
}

render() {
    // Clear canvas
    this.ctx.fillStyle = 'rgba(0, 4, 40, 0.1)';
    this.ctx.fillRect(0, 0, this.width, this.height);
    
    // Draw background elements
    this.drawBackground();
    
    // Draw game objects
    this.drawObstacles();
    this.drawAngryDogs();
    this.drawProjectiles();
    this.drawParticles();
    this.drawPowerUps();
    
    // Draw player and Nibble
    this.drawPlayer();
    this.drawNibble();
    
    // Draw game over screen
    if (this.gameState === 'gameOver') {
        this.drawGameOver();
    }
}

drawBackground() {
    // Draw cyberpunk city skyline
    this.ctx.fillStyle = 'rgba(0, 255, 255, 0.1)';
    for (let i = 0; i < 20; i++) {
        const x = (i * 100 - this.distance * 0.1) % (this.width + 100);
        const height = 50 + Math.sin(i) * 30;
        this.ctx.fillRect(x, this.height - height, 80, height);
    }
    
    // Draw neon grid
    this.ctx.strokeStyle = 'rgba(0, 255, 255, 0.2)';
    this.ctx.lineWidth = 1;
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
}

drawPlayer() {
    this.ctx.fillStyle = this.player.color;
    this.ctx.fillRect(this.player.x, this.player.y, this.player.width, this.player.height);
    
    // Draw player details
    this.ctx.fillStyle = '#ffffff';
    this.ctx.fillRect(this.player.x + 10, this.player.y + 10, 20, 20); // Head
    this.ctx.fillRect(this.player.x + 15, this.player.y + 30, 10, 30); // Body
}

drawNibble() {
    this.ctx.fillStyle = this.nibble.color;
    this.ctx.fillRect(this.nibble.x, this.nibble.y, this.nibble.width, this.nibble.height);
    
    // Draw Nibble details
    this.ctx.fillStyle = '#ffffff';
    this.ctx.fillRect(this.nibble.x + 5, this.nibble.y + 5, 15, 15); // Head
    this.ctx.fillRect(this.nibble.x + 10, this.nibble.y + 20, 5, 5); // Body
}

drawObstacles() {
    this.obstacles.forEach(obstacle => {
        this.ctx.fillStyle = obstacle.color;
        this.ctx.fillRect(obstacle.x, obstacle.y, obstacle.width, obstacle.height);
        
        // Draw obstacle type indicator
        this.ctx.fillStyle = '#ffffff';
        this.ctx.font = '12px Arial';
        this.ctx.fillText(obstacle.type.substring(0, 3).toUpperCase(), obstacle.x + 5, obstacle.y + 15);
    });
}

drawAngryDogs() {
    this.angryDogs.forEach(dog => {
        this.ctx.fillStyle = dog.color;
        this.ctx.fillRect(dog.x, dog.y, dog.width, dog.height);
        
        // Draw angry face
        this.ctx.fillStyle = '#ffffff';
        this.ctx.fillRect(dog.x + 5, dog.y + 5, 10, 10); // Head
        this.ctx.fillStyle = '#ff0000';
        this.ctx.fillRect(dog.x + 8, dog.y + 8, 2, 2); // Angry eyes
        this.ctx.fillRect(dog.x + 12, dog.y + 8, 2, 2);
    });
}

drawProjectiles() {
    this.projectiles.forEach(projectile => {
        this.ctx.fillStyle = projectile.color;
        this.ctx.fillRect(projectile.x, projectile.y, projectile.width, projectile.height);
    });
}

drawParticles() {
    this.particles.forEach(particle => {
        this.ctx.globalAlpha = particle.alpha;
        this.ctx.fillStyle = particle.color;
        this.ctx.fillRect(particle.x, particle.y, 2, 2);
        this.ctx.globalAlpha = 1;
    });
}

drawPowerUps() {
    this.powerUps.forEach(powerUp => {
        this.ctx.save();
        this.ctx.translate(powerUp.x + powerUp.width / 2, powerUp.y + powerUp.height / 2);
        this.ctx.rotate(powerUp.rotation);
        this.ctx.fillStyle = powerUp.color;
        this.ctx.fillRect(-powerUp.width / 2, -powerUp.height / 2, powerUp.width, powerUp.height);
        this.ctx.restore();
    });
}

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

gameLoop() {
    this.update();
    this.render();
    requestAnimationFrame(() => this.gameLoop());
}
}

// Start the game when the page loads
window.addEventListener('load', () => {
    new Game();
});