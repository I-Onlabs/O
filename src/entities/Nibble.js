import GameObject from '../core/GameObject.js';

/**
 * Nibble class - represents the loyal companion dog
 */
class Nibble extends GameObject {
    constructor(x, y) {
        super(x, y, 25, 25);
        
        // Nibble-specific properties
        this.health = 100;
        this.maxHealth = 100;
        this.color = '#ff6b6b';
        
        // AI properties
        this.followSpeed = 0.1;
        this.isFetching = false;
        this.fetchTarget = null;
        this.fetchProgress = 0;
        
        // Abilities
        this.abilities = {
            howl: { cooldown: 5000, lastUsed: 0 },
            dig: { cooldown: 3000, lastUsed: 0 }
        };
    }

    /**
     * Update Nibble's AI behavior
     * @param {Object} player - Player object to follow
     * @param {number} deltaTime - Time elapsed since last update
     */
    update(player, deltaTime) {
        if (this.isFetching && this.fetchTarget) {
            this.updateFetching(deltaTime);
        } else {
            this.updateFollowing(player);
        }
        
        super.update(deltaTime);
    }

    /**
     * Update following behavior
     * @param {Object} player - Player object to follow
     */
    updateFollowing(player) {
        // Calculate target position (behind and to the side of player)
        const targetX = player.x - 20;
        const targetY = player.y + 20;
        
        // Move towards target with some lag for natural following
        this.x += (targetX - this.x) * this.followSpeed;
        this.y += (targetY - this.y) * this.followSpeed;
    }

    /**
     * Update fetching behavior
     * @param {number} deltaTime - Time elapsed since last update
     */
    updateFetching(deltaTime) {
        if (!this.fetchTarget) return;
        
        const dx = this.fetchTarget.x - this.x;
        const dy = this.fetchTarget.y - this.y;
        const distance = Math.sqrt(dx * dx + dy * dy);
        
        if (distance < 30) {
            // Reached the target
            this.completeFetch();
        } else {
            // Move towards target
            this.x += dx * 0.05;
            this.y += dy * 0.05;
            this.fetchProgress = Math.min(1, this.fetchProgress + deltaTime * 0.001);
        }
    }

    /**
     * Complete the fetch action
     */
    completeFetch() {
        this.isFetching = false;
        this.fetchTarget = null;
        this.fetchProgress = 0;
        this.heal(20); // Fetching restores health
    }

    /**
     * Start fetching a target
     * @param {GameObject} target - Target object to fetch
     */
    startFetching(target) {
        this.isFetching = true;
        this.fetchTarget = target;
        this.fetchProgress = 0;
    }

    /**
     * Use howl ability to slow enemies
     * @param {Array} enemies - Array of enemy objects
     * @returns {boolean} - True if ability was used
     */
    useHowl(enemies) {
        const now = Date.now();
        if (now - this.abilities.howl.lastUsed < this.abilities.howl.cooldown) {
            return false; // On cooldown
        }
        
        // Slow all nearby enemies
        enemies.forEach(enemy => {
            const distance = this.getDistanceTo(enemy);
            if (distance < 100) {
                enemy.speed *= 0.5; // Slow down
                setTimeout(() => {
                    if (enemy.speed) enemy.speed *= 2; // Restore speed
                }, 2000);
            }
        });
        
        this.abilities.howl.lastUsed = now;
        return true;
    }

    /**
     * Use dig ability to find hidden items
     * @returns {boolean} - True if ability was used
     */
    useDig() {
        const now = Date.now();
        if (now - this.abilities.dig.lastUsed < this.abilities.dig.cooldown) {
            return false; // On cooldown
        }
        
        // Dig animation and potential item discovery
        this.abilities.dig.lastUsed = now;
        return true;
    }

    /**
     * Heal Nibble
     * @param {number} amount - Amount of health to restore
     */
    heal(amount) {
        this.health = Math.min(this.maxHealth, this.health + amount);
    }

    /**
     * Take damage and check if Nibble dies
     * @param {number} damage - Amount of damage to take
     * @returns {boolean} - True if Nibble dies
     */
    takeDamage(damage) {
        super.takeDamage(damage);
        return this.health <= 0;
    }

    /**
     * Render Nibble with detailed graphics
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    render(ctx) {
        if (!this.isActive) return;
        
        // Draw Nibble body
        ctx.fillStyle = this.color;
        ctx.fillRect(this.x, this.y, this.width, this.height);
        
        // Draw Nibble details
        ctx.fillStyle = '#ffffff';
        ctx.fillRect(this.x + 5, this.y + 5, 15, 15); // Head
        ctx.fillRect(this.x + 10, this.y + 20, 5, 5); // Body
        
        // Draw fetching indicator
        if (this.isFetching) {
            this.renderFetchingIndicator(ctx);
        }
        
        // Draw health bar
        this.renderHealthBar(ctx);
        
        // Draw ability cooldowns
        this.renderAbilityIndicators(ctx);
    }

    /**
     * Render fetching progress indicator
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderFetchingIndicator(ctx) {
        const indicatorSize = 8;
        const indicatorY = this.y - 15;
        
        // Fetching progress bar
        ctx.fillStyle = '#ffffff';
        ctx.fillRect(this.x + this.width/2 - 20, indicatorY, 40, 3);
        
        ctx.fillStyle = '#00ff00';
        ctx.fillRect(this.x + this.width/2 - 20, indicatorY, 40 * this.fetchProgress, 3);
    }

    /**
     * Render health bar above Nibble
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderHealthBar(ctx) {
        const barWidth = this.width;
        const barHeight = 3;
        const barY = this.y - 8;
        
        // Background
        ctx.fillStyle = '#333333';
        ctx.fillRect(this.x, barY, barWidth, barHeight);
        
        // Health
        const healthPercent = this.health / this.maxHealth;
        ctx.fillStyle = healthPercent > 0.5 ? '#00ff00' : healthPercent > 0.25 ? '#ffff00' : '#ff0000';
        ctx.fillRect(this.x, barY, barWidth * healthPercent, barHeight);
    }

    /**
     * Render ability cooldown indicators
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderAbilityIndicators(ctx) {
        const now = Date.now();
        const indicatorY = this.y + this.height + 2;
        
        // Howl ability indicator
        const howlCooldown = (now - this.abilities.howl.lastUsed) / this.abilities.howl.cooldown;
        ctx.fillStyle = howlCooldown >= 1 ? '#00ff00' : '#ff0000';
        ctx.fillRect(this.x, indicatorY, 8, 3);
        
        // Dig ability indicator
        const digCooldown = (now - this.abilities.dig.lastUsed) / this.abilities.dig.cooldown;
        ctx.fillStyle = digCooldown >= 1 ? '#00ff00' : '#ff0000';
        ctx.fillRect(this.x + 10, indicatorY, 8, 3);
    }
}

export default Nibble;