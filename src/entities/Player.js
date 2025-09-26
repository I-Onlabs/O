import GameObject from '../core/GameObject.js';

/**
 * Player class - represents Riley, the main character
 */
class Player extends GameObject {
    constructor(x, y) {
        super(x, y, 40, 60);
        
        // Player-specific properties
        this.health = 100;
        this.maxHealth = 100;
        this.ammo = 10;
        this.maxAmmo = 10;
        this.ammoRegen = 0.02;
        this.color = '#00ffff';
        
        // Movement properties
        this.speed = 3;
        this.bounds = { left: 50, right: 750, top: 50, bottom: 550 };
    }

    /**
     * Update player state
     * @param {Object} keys - Object containing pressed keys
     * @param {number} deltaTime - Time elapsed since last update
     */
    update(keys, deltaTime) {
        // Handle movement input
        this.handleMovement(keys);
        
        // Apply movement
        super.update(deltaTime);
        
        // Keep player in bounds
        this.constrainToBounds();
        
        // Regenerate ammo
        this.regenerateAmmo();
    }

    /**
     * Handle movement input
     * @param {Object} keys - Object containing pressed keys
     */
    handleMovement(keys) {
        // Vertical movement
        if (keys['ArrowUp'] || keys['KeyW']) {
            this.vy = -this.speed;
        } else if (keys['ArrowDown'] || keys['KeyS']) {
            this.vy = this.speed;
        } else {
            this.vy *= 0.8; // Damping
        }
        
        // Horizontal movement
        if (keys['ArrowLeft'] || keys['KeyA']) {
            this.vx = -this.speed * 0.7;
        } else if (keys['ArrowRight'] || keys['KeyD']) {
            this.vx = this.speed * 0.7;
        } else {
            this.vx *= 0.8; // Damping
        }
    }

    /**
     * Keep player within screen bounds
     */
    constrainToBounds() {
        this.x = Math.max(this.bounds.left, Math.min(this.bounds.right, this.x));
        this.y = Math.max(this.bounds.top, Math.min(this.bounds.bottom, this.y));
    }

    /**
     * Regenerate ammo over time
     */
    regenerateAmmo() {
        if (this.ammo < this.maxAmmo) {
            this.ammo += this.ammoRegen;
            if (this.ammo > this.maxAmmo) {
                this.ammo = this.maxAmmo;
            }
        }
    }

    /**
     * Check if player can shoot
     * @returns {boolean} - True if player has ammo
     */
    canShoot() {
        return this.ammo >= 1;
    }

    /**
     * Consume ammo for shooting
     * @returns {boolean} - True if ammo was consumed successfully
     */
    consumeAmmo() {
        if (this.canShoot()) {
            this.ammo -= 1;
            return true;
        }
        return false;
    }

    /**
     * Add ammo
     * @param {number} amount - Amount of ammo to add
     */
    addAmmo(amount) {
        this.ammo = Math.min(this.maxAmmo, this.ammo + amount);
    }

    /**
     * Heal the player
     * @param {number} amount - Amount of health to restore
     */
    heal(amount) {
        this.health = Math.min(this.maxHealth, this.health + amount);
    }

    /**
     * Take damage and check if player dies
     * @param {number} damage - Amount of damage to take
     * @returns {boolean} - True if player dies
     */
    takeDamage(damage) {
        super.takeDamage(damage);
        return this.health <= 0;
    }

    /**
     * Render the player with detailed graphics
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    render(ctx) {
        if (!this.isActive) return;
        
        // Draw cyberpunk glow effect
        ctx.shadowColor = this.color;
        ctx.shadowBlur = 15;
        
        // Draw player body
        ctx.fillStyle = this.color;
        ctx.fillRect(this.x, this.y, this.width, this.height);
        
        // Draw cyberpunk details
        ctx.shadowBlur = 0;
        ctx.fillStyle = '#ffffff';
        ctx.fillRect(this.x + 10, this.y + 10, 20, 20); // Head
        
        // Cyberpunk visor
        ctx.fillStyle = '#00ffff';
        ctx.fillRect(this.x + 12, this.y + 12, 16, 8);
        
        // Body with tech details
        ctx.fillStyle = '#333333';
        ctx.fillRect(this.x + 15, this.y + 30, 10, 30);
        
        // Tech lines
        ctx.strokeStyle = '#00ffff';
        ctx.lineWidth = 1;
        ctx.beginPath();
        ctx.moveTo(this.x + 5, this.y + 35);
        ctx.lineTo(this.x + 35, this.y + 35);
        ctx.moveTo(this.x + 5, this.y + 45);
        ctx.lineTo(this.x + 35, this.y + 45);
        ctx.stroke();
        
        // Draw health bar
        this.renderHealthBar(ctx);
    }

    /**
     * Render health bar above player
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderHealthBar(ctx) {
        const barWidth = this.width;
        const barHeight = 4;
        const barY = this.y - 8;
        
        // Background
        ctx.fillStyle = '#333333';
        ctx.fillRect(this.x, barY, barWidth, barHeight);
        
        // Health
        const healthPercent = this.health / this.maxHealth;
        ctx.fillStyle = healthPercent > 0.5 ? '#00ff00' : healthPercent > 0.25 ? '#ffff00' : '#ff0000';
        ctx.fillRect(this.x, barY, barWidth * healthPercent, barHeight);
    }
}

export default Player;