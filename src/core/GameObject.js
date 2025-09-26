/**
 * Base class for all game objects
 * Provides common properties and methods for entities in the game
 */
class GameObject {
    constructor(x, y, width, height) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.vx = 0;
        this.vy = 0;
        this.health = 1;
        this.maxHealth = 1;
        this.isActive = true;
        this.color = '#ffffff';
    }

    /**
     * Update the object's state
     * @param {number} deltaTime - Time elapsed since last update
     */
    update(deltaTime) {
        this.x += this.vx;
        this.y += this.vy;
    }

    /**
     * Render the object
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    render(ctx) {
        if (!this.isActive) return;
        
        ctx.fillStyle = this.color;
        ctx.fillRect(this.x, this.y, this.width, this.height);
    }

    /**
     * Check collision with another object
     * @param {GameObject} other - Other object to check collision with
     * @returns {boolean} - True if collision detected
     */
    checkCollision(other) {
        return this.x < other.x + other.width &&
               this.x + this.width > other.x &&
               this.y < other.y + other.height &&
               this.y + this.height > other.y;
    }

    /**
     * Take damage
     * @param {number} damage - Amount of damage to take
     */
    takeDamage(damage) {
        this.health -= damage;
        if (this.health <= 0) {
            this.health = 0;
            this.isActive = false;
        }
    }

    /**
     * Get distance to another object
     * @param {GameObject} other - Other object
     * @returns {number} - Distance between objects
     */
    getDistanceTo(other) {
        const dx = other.x - this.x;
        const dy = other.y - this.y;
        return Math.sqrt(dx * dx + dy * dy);
    }

    /**
     * Get center position
     * @returns {Object} - Object with x and y center coordinates
     */
    getCenter() {
        return {
            x: this.x + this.width / 2,
            y: this.y + this.height / 2
        };
    }
}

export default GameObject;