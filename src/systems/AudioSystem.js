/**
 * Audio System - manages sound effects and music
 * Uses Web Audio API for better performance and control
 */
class AudioSystem {
    constructor() {
        this.audioContext = null;
        this.sounds = new Map();
        this.musicVolume = 0.3;
        this.sfxVolume = 0.5;
        this.isMuted = false;
        
        this.initializeAudio();
    }

    /**
     * Initialize Web Audio API
     */
    initializeAudio() {
        try {
            this.audioContext = new (window.AudioContext || window.webkitAudioContext)();
        } catch (error) {
            console.warn('Web Audio API not supported:', error);
        }
    }

    /**
     * Resume audio context (required for user interaction)
     */
    resumeAudio() {
        if (this.audioContext && this.audioContext.state === 'suspended') {
            this.audioContext.resume();
        }
    }

    /**
     * Create a simple beep sound
     * @param {number} frequency - Sound frequency in Hz
     * @param {number} duration - Duration in seconds
     * @param {string} type - Wave type ('sine', 'square', 'sawtooth', 'triangle')
     */
    createBeep(frequency, duration, type = 'sine') {
        if (!this.audioContext || this.isMuted) return;

        const oscillator = this.audioContext.createOscillator();
        const gainNode = this.audioContext.createGain();

        oscillator.connect(gainNode);
        gainNode.connect(this.audioContext.destination);

        oscillator.frequency.setValueAtTime(frequency, this.audioContext.currentTime);
        oscillator.type = type;

        gainNode.gain.setValueAtTime(0, this.audioContext.currentTime);
        gainNode.gain.linearRampToValueAtTime(this.sfxVolume * 0.1, this.audioContext.currentTime + 0.01);
        gainNode.gain.exponentialRampToValueAtTime(0.001, this.audioContext.currentTime + duration);

        oscillator.start(this.audioContext.currentTime);
        oscillator.stop(this.audioContext.currentTime + duration);
    }

    /**
     * Play shooting sound
     */
    playShoot() {
        this.createBeep(800, 0.1, 'square');
    }

    /**
     * Play explosion sound
     */
    playExplosion() {
        this.createBeep(200, 0.3, 'sawtooth');
        setTimeout(() => this.createBeep(150, 0.2, 'sawtooth'), 100);
    }

    /**
     * Play damage sound
     */
    playDamage() {
        this.createBeep(300, 0.2, 'triangle');
    }

    /**
     * Play power-up collection sound
     */
    playPowerUp() {
        this.createBeep(600, 0.15, 'sine');
        setTimeout(() => this.createBeep(800, 0.15, 'sine'), 75);
        setTimeout(() => this.createBeep(1000, 0.15, 'sine'), 150);
    }

    /**
     * Play Nibble howl sound
     */
    playHowl() {
        this.createBeep(400, 0.5, 'sine');
        setTimeout(() => this.createBeep(350, 0.3, 'sine'), 200);
    }

    /**
     * Play game over sound
     */
    playGameOver() {
        this.createBeep(200, 0.5, 'sawtooth');
        setTimeout(() => this.createBeep(150, 0.5, 'sawtooth'), 300);
        setTimeout(() => this.createBeep(100, 0.5, 'sawtooth'), 600);
    }

    /**
     * Play background music (simple loop)
     */
    playBackgroundMusic() {
        if (!this.audioContext || this.isMuted) return;

        const playTone = (frequency, duration, delay = 0) => {
            setTimeout(() => {
                const oscillator = this.audioContext.createOscillator();
                const gainNode = this.audioContext.createGain();

                oscillator.connect(gainNode);
                gainNode.connect(this.audioContext.destination);

                oscillator.frequency.setValueAtTime(frequency, this.audioContext.currentTime);
                oscillator.type = 'sine';

                gainNode.gain.setValueAtTime(0, this.audioContext.currentTime);
                gainNode.gain.linearRampToValueAtTime(this.musicVolume * 0.05, this.audioContext.currentTime + 0.01);
                gainNode.gain.exponentialRampToValueAtTime(0.001, this.audioContext.currentTime + duration);

                oscillator.start(this.audioContext.currentTime);
                oscillator.stop(this.audioContext.currentTime + duration);
            }, delay);
        };

        // Simple cyberpunk melody
        const melody = [
            { freq: 440, duration: 0.3 }, // A
            { freq: 554, duration: 0.3 }, // C#
            { freq: 659, duration: 0.3 }, // E
            { freq: 880, duration: 0.6 }, // A (octave)
            { freq: 659, duration: 0.3 }, // E
            { freq: 554, duration: 0.3 }, // C#
            { freq: 440, duration: 0.6 }  // A
        ];

        melody.forEach((note, index) => {
            playTone(note.freq, note.duration, index * 400);
        });

        // Loop the melody
        setTimeout(() => this.playBackgroundMusic(), melody.length * 400 + 1000);
    }

    /**
     * Play obstacle destruction sound based on type
     * @param {string} obstacleType - Type of obstacle destroyed
     */
    playObstacleDestroyed(obstacleType) {
        switch (obstacleType) {
            case 'bouncyBoneDrone':
                this.createBeep(500, 0.2, 'square');
                break;
            case 'slobberMine':
                this.createBeep(300, 0.3, 'sawtooth');
                break;
            case 'holoFleaSwarm':
                this.createBeep(800, 0.1, 'triangle');
                setTimeout(() => this.createBeep(1000, 0.1, 'triangle'), 50);
                break;
            case 'runawayDogWalker':
                this.createBeep(400, 0.4, 'square');
                break;
            case 'kibbleCannon':
                this.createBeep(600, 0.3, 'sawtooth');
                break;
            case 'neonChewToy':
                this.createBeep(700, 0.2, 'sine');
                break;
            default:
                this.playExplosion();
        }
    }

    /**
     * Toggle mute state
     */
    toggleMute() {
        this.isMuted = !this.isMuted;
        return this.isMuted;
    }

    /**
     * Set music volume
     * @param {number} volume - Volume level (0-1)
     */
    setMusicVolume(volume) {
        this.musicVolume = Math.max(0, Math.min(1, volume));
    }

    /**
     * Set sound effects volume
     * @param {number} volume - Volume level (0-1)
     */
    setSfxVolume(volume) {
        this.sfxVolume = Math.max(0, Math.min(1, volume));
    }

    /**
     * Stop all audio
     */
    stopAll() {
        if (this.audioContext) {
            this.audioContext.close();
            this.initializeAudio();
        }
    }
}

export default AudioSystem;