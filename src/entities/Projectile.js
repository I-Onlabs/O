import GameObject from '../core/GameObject.js';

/**
 * Projectile class - represents shots fired by the player
 */
class Projectile extends GameObject {
    constructor(x, y, vx, vy) {
        super(x, y, 8, 4);
        
        // Projectile properties
        this.vx = vx;
        this.vy = vy;
        this.damage = 1;
        this.color = '#ffff00';
        this.lifetime = 120; // frames
        this.maxLifetime = 120;
        this.speed = 8;
        
        // Visual effects
        this.trail = [];
        this.trailLength = 5;
    }

    /**
     * Update projectile state
     * @param {number} deltaTime - Time elapsed since last update
     */
    update(deltaTime) {
        // Update position
        super.update(deltaTime);
        
        // Update trail
        this.updateTrail();
        
        // Decrease lifetime
        this.lifetime--;
        if (this.lifetime <= 0) {
            this.isActive = false;
        }
    }

    /**
     * Update trail effect
     */
    updateTrail() {
        // Add current position to trail
        this.trail.push({
            x: this.x,
            y: this.y,
            alpha: 1
        });
        
        // Limit trail length
        if (this.trail.length > this.trailLength) {
            this.trail.shift();
        }
        
        // Fade trail
        this.trail.forEach(point => {
            point.alpha *= 0.8;
        });
    }

    /**
     * Check if projectile is off-screen
     * @param {number} screenWidth - Width of the game screen
     * @param {number} screenHeight - Height of the game screen
     * @returns {boolean} - True if off-screen
     */
    isOffScreen(screenWidth, screenHeight) {
        return this.x > screenWidth || 
               this.x + this.width < 0 || 
               this.y > screenHeight || 
               this.y + this.height < 0;
    }

    /**
     * Get damage amount
     * @returns {number} - Damage this projectile deals
     */
    getDamage() {
        return this.damage;
    }

    /**
     * Render the projectile with trail effect
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    render(ctx) {
        if (!this.isActive) return;
        
        // Render trail
        this.renderTrail(ctx);
        
        // Render main projectile
        ctx.fillStyle = this.color;
        ctx.fillRect(this.x, this.y, this.width, this.height);
        
        // Add glow effect
        ctx.shadowColor = this.color;
        ctx.shadowBlur = 5;
        ctx.fillRect(this.x, this.y, this.width, this.height);
        ctx.shadowBlur = 0;
    }

    /**
     * Render trail effect
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderTrail(ctx) {
        this.trail.forEach((point, index) => {
            ctx.globalAlpha = point.alpha;
            ctx.fillStyle = this.color;
            const size = (index + 1) * 0.5;
            ctx.fillRect(point.x, point.y, size, size);
        });
        ctx.globalAlpha = 1;
    }
}

export default Projectile;