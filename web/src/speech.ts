type VoiceClip = {
  file: string
  start: number
  duration: number
}

type VoiceManifest = Record<string, VoiceClip>

let audioContext: AudioContext | undefined
let manifestPromise: Promise<VoiceManifest> | undefined
let audioBufferPromise: Promise<AudioBuffer> | undefined
let audioBufferFile: string | undefined
let activeSource: AudioBufferSourceNode | undefined

function getAudioContext() {
  audioContext ??= new AudioContext()
  return audioContext
}

async function loadManifest() {
  manifestPromise ??= fetch('/voice/gosia/manifest.json').then(async (response) => {
    if (!response.ok) {
      throw new Error(`Nie udało się wczytać głosu Gosia: ${response.status}`)
    }
    return await response.json() as VoiceManifest
  })
  return await manifestPromise
}

async function loadAudioBuffer(context: AudioContext, file: string) {
  if (audioBufferFile !== file) {
    audioBufferFile = file
    audioBufferPromise = fetch(`/voice/gosia/${file}`)
      .then(async (response) => {
        if (!response.ok) {
          throw new Error(`Nie udało się wczytać nagrań głosu Gosia: ${response.status}`)
        }
        return await response.arrayBuffer()
      })
      .then((audio) => context.decodeAudioData(audio))
      .catch((error: unknown) => {
        if (audioBufferFile === file) {
          audioBufferFile = undefined
          audioBufferPromise = undefined
        }
        throw error
      })
  }
  if (!audioBufferPromise) {
    throw new Error('Nie udało się rozpocząć wczytywania nagrań głosu Gosia.')
  }
  return await audioBufferPromise
}

export async function playSpeech(text: string) {
  const context = getAudioContext()
  if (context.state === 'suspended') await context.resume()

  const manifest = await loadManifest()
  const clip = manifest[text]
  if (!clip) {
    throw new Error(`Brakuje nagrania dla tekstu: ${text}`)
  }
  const buffer = await loadAudioBuffer(context, clip.file)

  activeSource?.stop()
  const source = context.createBufferSource()
  source.buffer = buffer
  source.connect(context.destination)
  activeSource = source

  await new Promise<void>((resolve, reject) => {
    source.onended = () => {
      if (activeSource === source) activeSource = undefined
      resolve()
    }
    try {
      source.start(0, clip.start, clip.duration)
    } catch (error) {
      activeSource = undefined
      reject(error)
    }
  })
}
