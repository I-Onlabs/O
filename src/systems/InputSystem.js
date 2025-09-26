/**
 * Input System - handles all input events and state management
 */
class InputSystem {
    constructor() {
        this.keys = {};
        this.mouse = {
            x: 0,
            y: 0,
            left: false,
            right: false,
            middle: false
        };
        this.touch = {
            active: false,
            x: 0,
            y: 0,
            startX: 0,
            startY: 0
        };
        
        // Gesture detection
        this.gestures = {
            swipeThreshold: 50,
            tapThreshold: 200,
            lastTouchTime: 0,
            lastTouchX: 0,
            lastTouchY: 0
        };
        
        this.setupEventListeners();
    }

    /**
     * Set up all input event listeners
     */
    setupEventListeners() {
        // Keyboard events
        document.addEventListener('keydown', (e) => {
            this.keys[e.code] = true;
            this.onKeyDown(e);
        });

        document.addEventListener('keyup', (e) => {
            this.keys[e.code] = false;
            this.onKeyUp(e);
        });

        // Mouse events
        document.addEventListener('mousemove', (e) => {
            this.mouse.x = e.clientX;
            this.mouse.y = e.clientY;
            this.onMouseMove(e);
        });

        document.addEventListener('mousedown', (e) => {
            this.mouse[e.button === 0 ? 'left' : e.button === 1 ? 'middle' : 'right'] = true;
            this.onMouseDown(e);
        });

        document.addEventListener('mouseup', (e) => {
            this.mouse[e.button === 0 ? 'left' : e.button === 1 ? 'middle' : 'right'] = false;
            this.onMouseUp(e);
        });

        // Touch events
        document.addEventListener('touchstart', (e) => {
            e.preventDefault();
            if (e.touches.length > 0) {
                const touch = e.touches[0];
                this.touch.active = true;
                this.touch.x = touch.clientX;
                this.touch.y = touch.clientY;
                this.touch.startX = touch.clientX;
                this.touch.startY = touch.clientY;
                
                this.gestures.lastTouchTime = Date.now();
                this.gestures.lastTouchX = touch.clientX;
                this.gestures.lastTouchY = touch.clientY;
                
                this.onTouchStart(e);
            }
        });

        document.addEventListener('touchmove', (e) => {
            e.preventDefault();
            if (e.touches.length > 0) {
                const touch = e.touches[0];
                this.touch.x = touch.clientX;
                this.touch.y = touch.clientY;
                this.onTouchMove(e);
            }
        });

        document.addEventListener('touchend', (e) => {
            e.preventDefault();
            this.touch.active = false;
            this.onTouchEnd(e);
        });

        // Prevent context menu on long press
        document.addEventListener('contextmenu', (e) => {
            e.preventDefault();
        });
    }

    /**
     * Check if a key is currently pressed
     * @param {string} keyCode - Key code to check
     * @returns {boolean} - True if key is pressed
     */
    isKeyPressed(keyCode) {
        return !!this.keys[keyCode];
    }

    /**
     * Check if any movement keys are pressed
     * @returns {Object} - Movement state
     */
    getMovementInput() {
        return {
            up: this.isKeyPressed('ArrowUp') || this.isKeyPressed('KeyW'),
            down: this.isKeyPressed('ArrowDown') || this.isKeyPressed('KeyS'),
            left: this.isKeyPressed('ArrowLeft') || this.isKeyPressed('KeyA'),
            right: this.isKeyPressed('ArrowRight') || this.isKeyPressed('KeyD')
        };
    }

    /**
     * Check if shoot key is pressed
     * @returns {boolean} - True if shoot key is pressed
     */
    isShooting() {
        return this.isKeyPressed('Space') || this.mouse.left;
    }

    /**
     * Detect swipe gestures
     * @param {TouchEvent} e - Touch event
     * @returns {string|null} - Swipe direction or null
     */
    detectSwipe(e) {
        if (!this.touch.active) return null;

        const deltaX = this.touch.x - this.touch.startX;
        const deltaY = this.touch.y - this.touch.startY;
        const distance = Math.sqrt(deltaX * deltaX + deltaY * deltaY);

        if (distance < this.gestures.swipeThreshold) return null;

        const angle = Math.atan2(deltaY, deltaX) * 180 / Math.PI;

        if (angle > -45 && angle < 45) return 'right';
        if (angle > 45 && angle < 135) return 'down';
        if (angle > 135 || angle < -135) return 'left';
        if (angle > -135 && angle < -45) return 'up';

        return null;
    }

    /**
     * Detect tap gesture
     * @param {TouchEvent} e - Touch event
     * @returns {boolean} - True if tap detected
     */
    detectTap(e) {
        const timeDiff = Date.now() - this.gestures.lastTouchTime;
        const distance = Math.sqrt(
            Math.pow(this.touch.x - this.gestures.lastTouchX, 2) +
            Math.pow(this.touch.y - this.gestures.lastTouchY, 2)
        );

        return timeDiff < this.gestures.tapThreshold && distance < 20;
    }

    /**
     * Convert screen coordinates to game coordinates
     * @param {number} screenX - Screen X coordinate
     * @param {number} screenY - Screen Y coordinate
     * @param {HTMLCanvasElement} canvas - Game canvas
     * @returns {Object} - Game coordinates
     */
    screenToGameCoords(screenX, screenY, canvas) {
        const rect = canvas.getBoundingClientRect();
        return {
            x: screenX - rect.left,
            y: screenY - rect.top
        };
    }

    // Event handlers (can be overridden)
    onKeyDown(e) {
        // Override in game class
    }

    onKeyUp(e) {
        // Override in game class
    }

    onMouseMove(e) {
        // Override in game class
    }

    onMouseDown(e) {
        // Override in game class
    }

    onMouseUp(e) {
        // Override in game class
    }

    onTouchStart(e) {
        // Override in game class
    }

    onTouchMove(e) {
        // Override in game class
    }

    onTouchEnd(e) {
        // Override in game class
    }

    /**
     * Get input summary for debugging
     * @returns {Object} - Current input state
     */
    getInputState() {
        return {
            keys: { ...this.keys },
            mouse: { ...this.mouse },
            touch: { ...this.touch },
            movement: this.getMovementInput(),
            shooting: this.isShooting()
        };
    }
}

export default InputSystem;