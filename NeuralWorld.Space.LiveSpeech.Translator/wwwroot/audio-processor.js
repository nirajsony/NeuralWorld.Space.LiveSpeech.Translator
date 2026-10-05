class AudioProcessor extends AudioWorkletProcessor {

    process(inputs) {

        const input = inputs[0];

        if (!input || input.length === 0) {
            return true;
        }

        const channelData = input[0];

        if (!channelData) {
            return true;
        }

        // Find the largest raw microphone sample.
        let maxAmplitude = 0;

        for (let i = 0; i < channelData.length; i++) {
            const value = Math.abs(channelData[i]);

            if (value > maxAmplitude) {
                maxAmplitude = value;
            }
        }

        // Convert Float32 → PCM16.
        const pcm16 = new Int16Array(channelData.length);

        for (let i = 0; i < channelData.length; i++) {

            const sample =
                Math.max(-1, Math.min(1, channelData[i]));

            pcm16[i] = sample < 0
                ? sample * 0x8000
                : sample * 0x7FFF;
        }

        this.port.postMessage(
            {
                buffer: pcm16.buffer,
                maxAmplitude
            },
            [pcm16.buffer]
        );

        return true;
    }
}

registerProcessor("audio-processor", AudioProcessor);