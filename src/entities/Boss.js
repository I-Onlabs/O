import GameObject from '../core/GameObject.js';

/**
 * Boss class - represents the Cyber-Chihuahua King
 * A pint-sized chihuahua in a massive mech-suit
 */
class Boss extends GameObject {
    constructor(x, y) {
        super(x, y, 120, 120);
        
        // Boss properties
        this.health = 50;
        this.maxHealth = 50;
        this.color = '#ff6b6b';
        this.speed = 1;
        this.phase = 1; // 1, 2, 3
        this.phaseHealthThresholds = [35, 20, 0];
        
        // Attack properties
        this.attackCooldown = 0;
        this.barkBombCooldown = 0;
        this.miniChihuahuaCooldown = 0;
        this.lastAttack = 0;
        
        // Movement properties
        this.movementPattern = 'hover';
        this.movementTimer = 0;
        this.targetY = y;
        
        // Visual effects
        this.animationTime = 0;
        this.mechGlow = 0;
        this.screenShake = 0;
        
        // Boss abilities
        this.abilities = {
            barkBomb: { cooldown: 3000, lastUsed: 0 },
            miniChihuahuas: { cooldown: 5000, lastUsed: 0 },
            yapOverload: { cooldown: 8000, lastUsed: 0 }
        };
        
        // Mini chihuahuas spawned
        this.miniChihuahuas = [];
    }

    /**
     * Update boss behavior
     * @param {Object} player - Player object
     * @param {Object} nibble - Nibble object
     * @param {number} deltaTime - Time elapsed since last update
     */
    update(player, nibble, deltaTime) {
        this.animationTime += deltaTime;
        this.movementTimer += deltaTime;
        
        // Update phase based on health
        this.updatePhase();
        
        // Update movement
        this.updateMovement(player, deltaTime);
        
        // Update attacks
        this.updateAttacks(player, nibble, deltaTime);
        
        // Update mini chihuahuas
        this.updateMiniChihuahuas(deltaTime);
        
        // Update visual effects
        this.updateVisualEffects(deltaTime);
        
        super.update(deltaTime);
    }

    /**
     * Update boss phase based on health
     */
    updatePhase() {
        if (this.health <= this.phaseHealthThresholds[0] && this.phase === 1) {
            this.phase = 2;
            this.speed *= 1.5;
            this.abilities.barkBomb.cooldown = 2000;
        } else if (this.health <= this.phaseHealthThresholds[1] && this.phase === 2) {
            this.phase = 3;
            this.speed *= 1.5;
            this.abilities.barkBomb.cooldown = 1500;
            this.abilities.miniChihuahuas.cooldown = 3000;
        }
    }

    /**
     * Update boss movement
     * @param {Object} player - Player object
     * @param {number} deltaTime - Time elapsed since last update
     */
    updateMovement(player, deltaTime) {
        // Hovering movement pattern
        this.targetY = player.y + Math.sin(this.movementTimer * 0.001) * 100;
        
        // Move towards target
        const dy = this.targetY - this.y;
        this.y += dy * 0.02;
        
        // Side-to-side movement
        this.x += Math.sin(this.movementTimer * 0.002) * 0.5;
        
        // Keep boss on screen
        this.x = Math.max(50, Math.min(650, this.x));
        this.y = Math.max(50, Math.min(450, this.y));
    }

    /**
     * Update boss attacks
     * @param {Object} player - Player object
     * @param {Object} nibble - Nibble object
     * @param {number} deltaTime - Time elapsed since last update
     */
    updateAttacks(player, nibble, deltaTime) {
        const now = Date.now();
        
        // Bark Bomb attack
        if (now - this.abilities.barkBomb.lastUsed > this.abilities.barkBomb.cooldown) {
            this.useBarkBomb(player);
            this.abilities.barkBomb.lastUsed = now;
        }
        
        // Mini Chihuahuas spawn
        if (now - this.abilities.miniChihuahuas.lastUsed > this.abilities.miniChihuahuas.cooldown) {
            this.spawnMiniChihuahuas();
            this.abilities.miniChihuahuas.lastUsed = now;
        }
        
        // Yap Overload (phase 3 only)
        if (this.phase === 3 && now - this.abilities.yapOverload.lastUsed > this.abilities.yapOverload.cooldown) {
            this.useYapOverload();
            this.abilities.yapOverload.lastUsed = now;
        }
    }

    /**
     * Use Bark Bomb attack
     * @param {Object} player - Player object
     */
    useBarkBomb(player) {
        // Create bark bomb projectiles
        const bombCount = this.phase >= 2 ? 3 : 2;
        for (let i = 0; i < bombCount; i++) {
            const angle = (i - (bombCount - 1) / 2) * 0.5;
            const bomb = {
                x: this.x + this.width / 2,
                y: this.y + this.height / 2,
                vx: Math.cos(angle) * 3,
                vy: Math.sin(angle) * 3,
                life: 120,
                maxLife: 120,
                damage: 20,
                radius: 30
            };
            
            // Store bomb for rendering and collision
            this.barkBombs = this.barkBombs || [];
            this.barkBombs.push(bomb);
        }
        
        // Screen shake effect
        this.screenShake = 10;
    }

    /**
     * Spawn mini chihuahuas
     */
    spawnMiniChihuahuas() {
        const count = this.phase >= 3 ? 4 : 2;
        for (let i = 0; i < count; i++) {
            const miniChihuahua = {
                x: this.x + Math.random() * this.width,
                y: this.y + Math.random() * this.height,
                vx: (Math.random() - 0.5) * 2,
                vy: (Math.random() - 0.5) * 2,
                health: 1,
                maxHealth: 1,
                size: 20,
                color: '#ff4757',
                life: 300 // 5 seconds
            };
            
            this.miniChihuahuas.push(miniChihuahua);
        }
    }

    /**
     * Use Yap Overload ability (phase 3)
     */
    useYapOverload() {
        // Create screen-wide shockwave
        this.screenShake = 20;
        
        // All enemies get stunned
        // This would affect all angry dogs on screen
        this.yapOverloadActive = true;
        setTimeout(() => {
            this.yapOverloadActive = false;
        }, 2000);
    }

    /**
     * Update mini chihuahuas
     * @param {number} deltaTime - Time elapsed since last update
     */
    updateMiniChihuahuas(deltaTime) {
        for (let i = this.miniChihuahuas.length - 1; i >= 0; i--) {
            const mini = this.miniChihuahuas[i];
            
            // Update position
            mini.x += mini.vx;
            mini.y += mini.vy;
            
            // Bounce off walls
            if (mini.x < 0 || mini.x > 800) mini.vx *= -1;
            if (mini.y < 0 || mini.y > 600) mini.vy *= -1;
            
            // Update life
            mini.life--;
            if (mini.life <= 0 || mini.health <= 0) {
                this.miniChihuahuas.splice(i, 1);
            }
        }
    }

    /**
     * Update visual effects
     * @param {number} deltaTime - Time elapsed since last update
     */
    updateVisualEffects(deltaTime) {
        // Mech glow effect
        this.mechGlow = Math.sin(this.animationTime * 0.01) * 0.5 + 0.5;
        
        // Screen shake decay
        if (this.screenShake > 0) {
            this.screenShake--;
        }
    }

    /**
     * Take damage and handle phase transitions
     * @param {number} damage - Amount of damage to take
     * @returns {boolean} - True if boss is defeated
     */
    takeDamage(damage) {
        super.takeDamage(damage);
        
        // Phase transition effects
        if (this.health <= this.phaseHealthThresholds[0] && this.phase === 1) {
            this.screenShake = 15;
        } else if (this.health <= this.phaseHealthThresholds[1] && this.phase === 2) {
            this.screenShake = 20;
        }
        
        return this.health <= 0;
    }

    /**
     * Render the boss with detailed graphics
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    render(ctx) {
        if (!this.isActive) return;
        
        // Apply screen shake
        if (this.screenShake > 0) {
            ctx.save();
            ctx.translate(
                (Math.random() - 0.5) * this.screenShake,
                (Math.random() - 0.5) * this.screenShake
            );
        }
        
        // Draw mech suit
        this.renderMechSuit(ctx);
        
        // Draw chihuahua
        this.renderChihuahua(ctx);
        
        // Draw phase effects
        this.renderPhaseEffects(ctx);
        
        // Draw mini chihuahuas
        this.renderMiniChihuahuas(ctx);
        
        // Draw bark bombs
        this.renderBarkBombs(ctx);
        
        // Draw health bar
        this.renderHealthBar(ctx);
        
        if (this.screenShake > 0) {
            ctx.restore();
        }
    }

    /**
     * Render the mech suit
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderMechSuit(ctx) {
        // Mech body with glow
        ctx.shadowColor = this.color;
        ctx.shadowBlur = 20 * this.mechGlow;
        ctx.fillStyle = this.color;
        ctx.fillRect(this.x, this.y, this.width, this.height);
        ctx.shadowBlur = 0;
        
        // Mech details
        ctx.fillStyle = '#333333';
        ctx.fillRect(this.x + 10, this.y + 10, 100, 100);
        
        // Tech lines
        ctx.strokeStyle = '#00ffff';
        ctx.lineWidth = 2;
        for (let i = 0; i < 5; i++) {
            ctx.beginPath();
            ctx.moveTo(this.x + 20, this.y + 20 + i * 20);
            ctx.lineTo(this.x + 100, this.y + 20 + i * 20);
            ctx.stroke();
        }
        
        // Phase indicator
        ctx.fillStyle = this.phase === 3 ? '#ff0000' : this.phase === 2 ? '#ff8800' : '#ffff00';
        ctx.fillRect(this.x + 5, this.y + 5, 10, 10);
    }

    /**
     * Render the chihuahua
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderChihuahua(ctx) {
        const chihuahuaX = this.x + this.width / 2 - 15;
        const chihuahuaY = this.y + this.height / 2 - 15;
        
        // Chihuahua body
        ctx.fillStyle = '#ff6b6b';
        ctx.fillRect(chihuahuaX, chihuahuaY, 30, 30);
        
        // Angry face
        ctx.fillStyle = '#ffffff';
        ctx.fillRect(chihuahuaX + 5, chihuahuaY + 5, 20, 20);
        
        // Angry eyes
        ctx.fillStyle = '#ff0000';
        ctx.fillRect(chihuahuaX + 8, chihuahuaY + 8, 4, 4);
        ctx.fillRect(chihuahuaX + 18, chihuahuaY + 8, 4, 4);
        
        // Snarling mouth
        ctx.fillStyle = '#000000';
        ctx.fillRect(chihuahuaX + 10, chihuahuaY + 15, 10, 2);
    }

    /**
     * Render phase-specific effects
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderPhaseEffects(ctx) {
        if (this.phase >= 2) {
            // Phase 2+ effects
            ctx.strokeStyle = '#ff0000';
            ctx.lineWidth = 3;
            ctx.strokeRect(this.x - 5, this.y - 5, this.width + 10, this.height + 10);
        }
        
        if (this.phase === 3) {
            // Phase 3 effects
            ctx.fillStyle = 'rgba(255, 0, 0, 0.2)';
            ctx.fillRect(0, 0, 800, 600);
        }
    }

    /**
     * Render mini chihuahuas
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderMiniChihuahuas(ctx) {
        this.miniChihuahuas.forEach(mini => {
            ctx.fillStyle = mini.color;
            ctx.fillRect(mini.x, mini.y, mini.size, mini.size);
            
            // Mini angry face
            ctx.fillStyle = '#ffffff';
            ctx.fillRect(mini.x + 3, mini.y + 3, 14, 14);
            
            ctx.fillStyle = '#ff0000';
            ctx.fillRect(mini.x + 5, mini.y + 5, 2, 2);
            ctx.fillRect(mini.x + 11, mini.y + 5, 2, 2);
        });
    }

    /**
     * Render bark bombs
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderBarkBombs(ctx) {
        if (!this.barkBombs) return;
        
        for (let i = this.barkBombs.length - 1; i >= 0; i--) {
            const bomb = this.barkBombs[i];
            
            // Update bomb
            bomb.x += bomb.vx;
            bomb.y += bomb.vy;
            bomb.life--;
            
            // Remove expired bombs
            if (bomb.life <= 0) {
                this.barkBombs.splice(i, 1);
                continue;
            }
            
            // Draw bomb
            ctx.fillStyle = '#ffff00';
            ctx.beginPath();
            ctx.arc(bomb.x, bomb.y, bomb.radius, 0, Math.PI * 2);
            ctx.fill();
            
            // Bomb warning
            ctx.strokeStyle = '#ff0000';
            ctx.lineWidth = 2;
            ctx.stroke();
        }
    }

    /**
     * Render boss health bar
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderHealthBar(ctx) {
        const barWidth = 200;
        const barHeight = 20;
        const barX = 300;
        const barY = 20;
        
        // Background
        ctx.fillStyle = '#333333';
        ctx.fillRect(barX, barY, barWidth, barHeight);
        
        // Health
        const healthPercent = this.health / this.maxHealth;
        ctx.fillStyle = healthPercent > 0.6 ? '#00ff00' : 
                        healthPercent > 0.3 ? '#ffff00' : '#ff0000';
        ctx.fillRect(barX, barY, barWidth * healthPercent, barHeight);
        
        // Border
        ctx.strokeStyle = '#ffffff';
        ctx.lineWidth = 2;
        ctx.strokeRect(barX, barY, barWidth, barHeight);
        
        // Boss name
        ctx.fillStyle = '#ffffff';
        ctx.font = '16px Arial';
        ctx.textAlign = 'center';
        ctx.fillText('CYBER-CHIHUAHUA KING', barX + barWidth / 2, barY - 5);
        ctx.textAlign = 'left';
    }

    /**
     * Check if boss is off-screen
     * @param {number} screenWidth - Screen width
     * @param {number} screenHeight - Screen height
     * @returns {boolean} - True if off-screen
     */
    shouldRemove(screenWidth, screenHeight) {
        return false; // Boss never removes itself
    }
}

export default Boss;