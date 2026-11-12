import { useEffect, useEffectEvent, useRef, useState, type FormEvent } from 'react'
import './App.css'
import { DrawingPad } from './components/DrawingPad'
import { MathZone } from './components/MathZone'
import { emptyMathProgress, type MathProgress, type MathTopic } from './components/mathProgress'
import { playSpeech } from './speech'
import {
  avatars,
  exerciseNames,
  letters,
  syllableWordCategories,
  syllableWords,
  type ExerciseType,
  type Letter,
  type Question,
} from './data/learningData'

type Profile = {
  id: string
  name: string
  avatar: string
}

type LetterProgress = {
  practices: number
  correct: number
  learned: boolean
  level: number
  listenedWords: number
  drawingAttempts: number
}

type HistoryEntry = {
  id: string
  at: string
  displayAt: string
  letter: string
  description: string
  wasCorrect: boolean
}

type StoredData = {
  profiles: Profile[]
  selectedProfileId: string | null
  progress: Record<string, Record<string, LetterProgress>>
  history: Record<string, HistoryEntry[]>
  mathProgress: Record<string, MathProgress>
}

const storageKey = 'learn-with-me.web.v1'
const mathTopics: MathTopic[] = ['numbers', 'counting', 'addition', 'subtraction', 'multiplication', 'division']

const emptyData: StoredData = {
  profiles: [],
  selectedProfileId: null,
  progress: {},
  history: {},
  mathProgress: {},
}

function getProgressLevel(practices: number, drawingAttempts = 0, listenedWords = 0) {
  const totalProgress = practices + drawingAttempts + listenedWords
  if (totalProgress >= 60) return 5
  if (totalProgress >= 45) return 4
  if (totalProgress >= 30) return 3
  if (totalProgress >= 18) return 2
  return 1
}

function hasLetterMastery(progress: Partial<LetterProgress> | undefined, totalCards: number) {
  if (!progress) return false
  const listenedWords = Number(progress.listenedWords ?? 0)
  const drawingAttempts = Number(progress.drawingAttempts ?? 0)
  const practices = Number(progress.practices ?? 0)
  const correct = Number(progress.correct ?? 0)
  const level = Number(progress.level ?? 1)

  return listenedWords >= totalCards && drawingAttempts >= 12 && practices >= 8 && correct >= 6 && level >= 3
}

function getStars(level: number) {
  return `${'★'.repeat(level)}${'☆'.repeat(5 - level)}`
}

function getLetterReadingCards(letter: Letter) {
  const genericWords = {
    A: ['auto', 'arbuz', 'ananas', 'awokado', 'akwarium'],
    B: ['balon', 'baba', 'bąk', 'bawół', 'brodzik'],
    C: ['cytryna', 'cymbał', 'chmura', 'ciastko', 'cukierek'],
    D: ['dom', 'delfin', 'dąb', 'dzwon', 'domek'],
    E: ['ekran', 'elipsa', 'emotikon', 'drzewo', 'elektrownia'],
    F: ['foka', 'filiżanka', 'flet', 'futro', 'fontanna'],
    G: ['gitara', 'garnek', 'gęś', 'góra', 'gwiazda'],
    H: ['huśtawka', 'huba', 'hotel', 'helikopter', 'hipopotam'],
    I: ['igła', 'islandia', 'inhibitor', 'imbryk', 'izabela'],
    J: ['jabłko', 'jaskółka', 'jeż', 'jajko', 'jastrząb'],
    K: ['kot', 'krowa', 'kasa', 'krzak', 'kwiat'],
    L: ['lis', 'latawiec', 'lampa', 'las', 'łódka'],
    M: ['miś', 'mama', 'mak', 'motyl', 'morze'],
    N: ['nos', 'natura', 'namiot', 'niedźwiedź', 'niebo'],
    O: ['okno', 'owca', 'ocean', 'ognisko', 'oliwka'],
    P: ['pies', 'płotek', 'parasol', 'paczka', 'ptak'],
    R: ['rower', 'rzeka', 'ryba', 'róża', 'robot'],
    S: ['słońce', 'sowa', 'sanie', 'słoneczko', 'skakanka'],
    T: ['tęcza', 'tętnik', 'tulipan', 'traktorek', 'torba'],
    U: ['ucho', 'ul', 'uśmiech', 'urlop', 'ufo'],
    W: ['wiewiórka', 'woda', 'włosy', 'wąż', 'wieloryb'],
    Y: ['yo-yo', 'yeti', 'yogurt', 'ysza', 'ypsilon'],
    Z: ['zebra', 'zegar', 'zając', 'żółw', 'zoo'],
  }

  const sampleWords = genericWords[letter.uppercase as keyof typeof genericWords] ?? [
    letter.word,
    letter.word,
    letter.word,
    letter.word,
    letter.word,
  ]

  return sampleWords.slice(0, 5).map((word, index) => ({
    id: `${letter.uppercase}-${index + 1}`,
    word,
    phrase: `${letter.uppercase} jak ${word}`,
  }))
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}

function isValidMathProgress(value: unknown): value is MathProgress {
  if (!isRecord(value) || !Number.isInteger(value.attempts) || Number(value.attempts) < 0 ||
      !Number.isInteger(value.correct) || Number(value.correct) < 0 || !isRecord(value.topics)) {
    return false
  }
  const topics = value.topics
  return mathTopics.every((topic) => {
    const topicProgress = topics[topic]
    return isRecord(topicProgress) &&
      Number.isInteger(topicProgress.attempts) && Number(topicProgress.attempts) >= 0 &&
      Number.isInteger(topicProgress.correct) && Number(topicProgress.correct) >= 0
  })
}

function normalizeMathProgress(value: unknown): Record<string, MathProgress> {
  if (!isRecord(value)) return {}
  return Object.fromEntries(
    Object.entries(value).map(([profileId, storedProgress]) => {
      if (!isRecord(storedProgress)) return [profileId, emptyMathProgress()]
      const progress = emptyMathProgress()
      progress.attempts = Math.max(0, Math.trunc(Number(storedProgress.attempts) || 0))
      progress.correct = Math.max(0, Math.trunc(Number(storedProgress.correct) || 0))
      if (isRecord(storedProgress.topics)) {
        for (const topic of mathTopics) {
          const storedTopic = storedProgress.topics[topic]
          if (!isRecord(storedTopic)) continue
          progress.topics[topic] = {
            attempts: Math.max(0, Math.trunc(Number(storedTopic.attempts) || 0)),
            correct: Math.max(0, Math.trunc(Number(storedTopic.correct) || 0)),
          }
        }
      }
      return [profileId, progress]
    }),
  )
}

function isStoredData(value: unknown): value is StoredData {
  if (
    !isRecord(value) ||
    !Array.isArray(value.profiles) ||
    (value.selectedProfileId !== null && typeof value.selectedProfileId !== 'string') ||
    !isRecord(value.progress) ||
    !isRecord(value.history) ||
    !isRecord(value.mathProgress)
  ) {
    return false
  }

  const validProfiles = value.profiles.every(
    (profile) =>
      isRecord(profile) &&
      typeof profile.id === 'string' &&
      typeof profile.name === 'string' &&
      typeof profile.avatar === 'string',
  )
  const validProgress = Object.values(value.progress).every(
    (profileProgress) =>
      isRecord(profileProgress) &&
      Object.values(profileProgress).every((progress) => {
        if (!isRecord(progress)) return false
        const levelValue = typeof progress.level === 'number' ? progress.level : undefined
        return (
          Number.isInteger(progress.practices) &&
          Number(progress.practices) >= 0 &&
          Number.isInteger(progress.correct) &&
          Number(progress.correct) >= 0 &&
          typeof progress.learned === 'boolean' &&
          Number.isInteger(progress.listenedWords) &&
          Number(progress.listenedWords) >= 0 &&
          Number.isInteger(progress.drawingAttempts) &&
          Number(progress.drawingAttempts) >= 0 &&
          (typeof levelValue === 'undefined' || (Number.isInteger(levelValue) && levelValue >= 1 && levelValue <= 5))
        )
      }),
  )
  const validHistory = Object.values(value.history).every(
    (entries) =>
      Array.isArray(entries) &&
      entries.every(
        (entry) =>
          isRecord(entry) &&
          typeof entry.id === 'string' &&
          typeof entry.at === 'string' &&
          typeof entry.displayAt === 'string' &&
          typeof entry.letter === 'string' &&
          typeof entry.description === 'string' &&
          typeof entry.wasCorrect === 'boolean',
      ),
  )
  const validMathProgress = Object.values(value.mathProgress).every(isValidMathProgress)

  return validProfiles && validProgress && validHistory && validMathProgress
}

function createHistoryEntry(
  letter: string,
  description: string,
  wasCorrect: boolean,
): HistoryEntry {
  const now = new Date()
  return {
    id: crypto.randomUUID(),
    at: now.toISOString(),
    displayAt: new Intl.DateTimeFormat('pl-PL', {
      day: '2-digit',
      month: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    }).format(now),
    letter,
    description,
    wasCorrect,
  }
}

function readStoredData(): { data: StoredData; error: string } {
  try {
    const stored = localStorage.getItem(storageKey)
    if (!stored) return { data: emptyData, error: '' }
    const parsed: unknown = JSON.parse(stored)
    if (!isRecord(parsed)) {
      throw new Error('Dane zapisane w przeglądarce mają nieprawidłowy format.')
    }

    const normalized: StoredData = {
      profiles: Array.isArray(parsed.profiles) ? parsed.profiles as Profile[] : [],
      selectedProfileId: typeof parsed.selectedProfileId === 'string' ? parsed.selectedProfileId : null,
      progress: isRecord(parsed.progress)
        ? Object.fromEntries(
            Object.entries(parsed.progress).map(([profileId, profileProgress]) => [
              profileId,
              isRecord(profileProgress)
                ? Object.fromEntries(
                    Object.entries(profileProgress).map(([letter, progress]) => [
                      letter,
                      isRecord(progress)
                        ? {
                            practices: Number(progress.practices) || 0,
                            correct: Number(progress.correct) || 0,
                            learned: Boolean(progress.learned),
                            level: getProgressLevel(Number(progress.practices) || 0, Number(progress.drawingAttempts) || 0),
                            listenedWords: Number(progress.listenedWords) || 0,
                            drawingAttempts: Number(progress.drawingAttempts) || 0,
                          }
                        : {
                            practices: 0,
                            correct: 0,
                            learned: false,
                            level: 1,
                            listenedWords: 0,
                            drawingAttempts: 0,
                          },
                    ]),
                  )
                : {},
            ]),
          )
        : {},
      history: isRecord(parsed.history) ? (parsed.history as StoredData['history']) : {},
      mathProgress: normalizeMathProgress(parsed.mathProgress),
    }

    if (!isStoredData(normalized)) {
      throw new Error('Dane zapisane w przeglądarce mają nieprawidłowy format.')
    }

    return { data: normalized, error: '' }
  } catch (error) {
    console.error('Nie udało się odczytać danych aplikacji.', error)
    return {
      data: emptyData,
      error: 'Nie udało się odczytać zapisanych danych z tej przeglądarki. Po wykonaniu zmian nowe dane zastąpią uszkodzony zapis.',
    }
  }
}

function getEligibleLetters(type: ExerciseType) {
  const eligibleLetters =
    type === 'first-letter'
      ? letters.filter((item) => item.word.toLocaleLowerCase('pl-PL').startsWith(item.lowercase))
      : letters
  return eligibleLetters
}

function createQuestion(type: ExerciseType, previouslyAsked: string[]): Question {
  const eligibleLetters = getEligibleLetters(type)
  const unseenLetters = eligibleLetters.filter((item) => !previouslyAsked.includes(item.uppercase))
  const nextCycleLetters = eligibleLetters.filter((item) => item.uppercase !== previouslyAsked.at(-1))
  const candidates = unseenLetters.length > 0
    ? unseenLetters
    : nextCycleLetters.length > 0
      ? nextCycleLetters
      : eligibleLetters
  const letter = candidates[Math.floor(Math.random() * candidates.length)]
  const otherLetters = letters.filter((item) => item.uppercase !== letter.uppercase)
  const distractors = [...otherLetters].sort(() => Math.random() - 0.5).slice(0, 3)
  const answer =
    type === 'match-word'
      ? letter.illustration
      : type === 'match-case'
        ? letter.lowercase
        : type === 'keyboard'
          ? letter.uppercase
          : letter.uppercase
  const values =
    type === 'match-word'
      ? distractors.map((item) => item.illustration)
      : type === 'match-case'
        ? distractors.map((item) => item.lowercase)
        : type === 'keyboard'
          ? []
          : distractors.map((item) => item.uppercase)

  let prompt: string
  switch (type) {
    case 'find-letter':
      prompt = `Znajdź literę ${letter.uppercase}`
      break
    case 'match-word':
      prompt = `Wybierz obrazek do słowa „${letter.word}”`
      break
    case 'first-letter':
      prompt = `Na jaką literę zaczyna się słowo „${letter.word}”?`
      break
    case 'match-case':
      prompt = `Która mała litera pasuje do ${letter.uppercase}?`
      break
    case 'keyboard':
      prompt = 'Usłysz literę i naciśnij ją na klawiaturze.'
      break
  }

  return {
    type,
    prompt,
    answer,
    options: [...values, answer].sort(() => Math.random() - 0.5),
    letter,
  }
}

function getQuestionSpeech(question: Question) {
  if (question.type === 'match-word' || question.type === 'first-letter') {
    return `${question.letter.uppercase} jak ${question.letter.word}.`
  }
  return `Litera ${question.letter.uppercase}`
}

function App() {
  const [initialStorage] = useState(readStoredData)
  const [data, setData] = useState<StoredData>(initialStorage.data)
  const [storageError, setStorageError] = useState(initialStorage.error)
  const persistenceReady = useRef(!initialStorage.error)
  const [activeTab, setActiveTab] = useState<'discover' | 'practice' | 'progress' | 'syllables' | 'math'>('discover')
  const [selectedLetter, setSelectedLetter] = useState(letters[0])
  const [newName, setNewName] = useState('')
  const [newAvatar, setNewAvatar] = useState(avatars[0])
  const [drawVariant, setDrawVariant] = useState<'uppercase' | 'lowercase'>('uppercase')
  const [role, setRole] = useState<'Dziecko' | 'Rodzic'>('Dziecko')
  const [syllableCategory, setSyllableCategory] = useState<'all' | string>('all')
  const [syllableLength, setSyllableLength] = useState<'all' | number>('all')
  const [exerciseType, setExerciseType] = useState<ExerciseType>('find-letter')
  const [question, setQuestion] = useState<Question | null>(null)
  const [answered, setAnswered] = useState<string | null>(null)
  const [isSpeakingSyllables, setIsSpeakingSyllables] = useState(false)
  const [feedback, setFeedback] = useState('')
  const [feedbackTone, setFeedbackTone] = useState<'neutral' | 'success' | 'error'>('neutral')
  const [formError, setFormError] = useState('')
  const [isDrawingOpen, setIsDrawingOpen] = useState(false)
  const drawingTriggerRef = useRef<HTMLButtonElement>(null)
  const closeDrawingRef = useRef<HTMLButtonElement>(null)
  const askedLettersRef = useRef<Partial<Record<ExerciseType, string[]>>>({})
  const answerInProgressRef = useRef(false)
  const syllableSpeechInProgressRef = useRef(false)
  const nextQuestionTimeoutRef = useRef<number | null>(null)

  useEffect(() => {
    if (!persistenceReady.current) return
    try {
      localStorage.setItem(storageKey, JSON.stringify(data))
    } catch (error) {
      console.error('Nie udało się zapisać danych aplikacji.', error)
      window.setTimeout(
        () => setStorageError('Nie udało się zapisać danych w tej przeglądarce. Sprawdź wolne miejsce lub ustawienia prywatności.'),
        0,
      )
    }
  }, [data])

  useEffect(() => {
    if (!isDrawingOpen) return
    const previousOverflow = document.body.style.overflow
    const drawingTrigger = drawingTriggerRef.current
    document.body.style.overflow = 'hidden'
    closeDrawingRef.current?.focus()
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') setIsDrawingOpen(false)
    }
    window.addEventListener('keydown', handleKeyDown)
    return () => {
      window.removeEventListener('keydown', handleKeyDown)
      document.body.style.overflow = previousOverflow
      drawingTrigger?.focus()
    }
  }, [isDrawingOpen])

  useEffect(() => () => {
    if (nextQuestionTimeoutRef.current !== null) {
      window.clearTimeout(nextQuestionTimeoutRef.current)
    }
  }, [])

  const selectedProfile = data.profiles.find((profile) => profile.id === data.selectedProfileId)
  const profileProgress = selectedProfile ? data.progress[selectedProfile.id] ?? {} : {}
  const selectedMathProgress = selectedProfile
    ? data.mathProgress[selectedProfile.id] ?? emptyMathProgress()
    : emptyMathProgress()
  const learnedCount = Object.values(profileProgress).filter((item) => item.level >= 3 || item.learned).length
  const practiceCount = Object.values(profileProgress).reduce((sum, item) => sum + item.practices, 0)
  const isMathTab = activeTab === 'math'
  const history = selectedProfile ? data.history[selectedProfile.id] ?? [] : []
  const currentQuestion = question
  const isCurrentAnswerCorrect =
    currentQuestion && answered
      ? answered.trim().toLowerCase() === currentQuestion.answer.trim().toLowerCase()
      : false
  const badge =
    learnedCount >= letters.length
      ? '🏅 Mistrz alfabetu — poznaliście wszystkie litery!'
      : learnedCount >= 15
        ? '🌟 Wielki odkrywca — już ponad połowa alfabetu!'
        : learnedCount >= 5
          ? '🎈 Ciekawski odkrywca — pierwsze litery poznane!'
          : learnedCount >= 1
            ? '⭐ Pierwszy krok — każda litera to nowe odkrycie!'
            : 'Każda próba się liczy — zacznijcie od wybranej litery.'

  const selectedLetterProgress = selectedProfile
    ? profileProgress[selectedLetter.uppercase]
    : undefined

  const visibleSyllableWords = syllableWords.filter((item) => {
    const matchesCategory = syllableCategory === 'all' || item.category === syllableCategory
    const matchesLength = syllableLength === 'all' || item.syllableCount === syllableLength
    return matchesCategory && matchesLength
  })

  const progressPercent = Math.round((learnedCount / letters.length) * 100)
  const mathProgressPercent = selectedMathProgress.attempts > 0
    ? Math.round((selectedMathProgress.correct / selectedMathProgress.attempts) * 100)
    : 0
  const mathBadge = selectedMathProgress.attempts > 0
    ? `Każda próba się liczy — macie już ${selectedMathProgress.correct} udanych odpowiedzi!`
    : 'Każda próba to krok naprzód — wybierzcie pierwsze zadanie.'

  function setFeedbackMessage(message: string, tone: 'neutral' | 'success' | 'error' = 'neutral') {
    setFeedback(message)
    setFeedbackTone(tone)
  }

  function updateData(updater: (current: StoredData) => StoredData) {
    persistenceReady.current = true
    setStorageError('')
    setData((current) => updater(current))
  }

  function addProfile(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const name = newName.trim()
    if (!name || name.length > 30) {
      setFormError('Imię powinno mieć od 1 do 30 znaków.')
      return
    }
    const profile: Profile = { id: crypto.randomUUID(), name, avatar: newAvatar }
    updateData((current) => ({
      ...current,
      profiles: [...current.profiles, profile],
      selectedProfileId: profile.id,
    }))
    setNewName('')
    setFormError('')
    setFeedbackMessage('')
  }

  function selectProfile(id: string) {
    if (nextQuestionTimeoutRef.current !== null) {
      window.clearTimeout(nextQuestionTimeoutRef.current)
      nextQuestionTimeoutRef.current = null
    }
    updateData((current) => ({ ...current, selectedProfileId: id }))
    setFeedbackMessage('')
    setQuestion(null)
    setAnswered(null)
    askedLettersRef.current = {}
    answerInProgressRef.current = false
  }

  function deleteProfile(profile: Profile) {
    const confirmed = window.confirm(
      `Usunąć profil ${profile.name} razem z jego postępami i historią ćwiczeń? Tej czynności nie można cofnąć.`,
    )
    if (!confirmed) return
    if (nextQuestionTimeoutRef.current !== null) {
      window.clearTimeout(nextQuestionTimeoutRef.current)
      nextQuestionTimeoutRef.current = null
    }

    updateData((current) => {
      const remainingProfiles = current.profiles.filter((item) => item.id !== profile.id)
      return {
        ...current,
        profiles: remainingProfiles,
        selectedProfileId: current.selectedProfileId === profile.id
          ? remainingProfiles[0]?.id ?? null
          : current.selectedProfileId,
        progress: Object.fromEntries(
          Object.entries(current.progress).filter(([profileId]) => profileId !== profile.id),
        ),
        history: Object.fromEntries(
          Object.entries(current.history).filter(([profileId]) => profileId !== profile.id),
        ),
        mathProgress: Object.fromEntries(
          Object.entries(current.mathProgress).filter(([profileId]) => profileId !== profile.id),
        ),
      }
    })
    if (data.selectedProfileId === profile.id) setIsDrawingOpen(false)
    setQuestion(null)
    setAnswered(null)
    askedLettersRef.current = {}
    answerInProgressRef.current = false
    setFeedbackMessage('Profil i jego postępy zostały usunięte.')
  }

  function recordPractice(
    letter: string,
    description: string,
    wasCorrect: boolean,
    markLearned = false,
  ) {
    if (!selectedProfile) return
    const profileId = selectedProfile.id
    const entry = createHistoryEntry(letter, description, wasCorrect)
    updateData((current) => {
      const profileProgressData = current.progress[profileId] ?? {}
      const oldProgress = profileProgressData[letter] ?? {
        practices: 0,
        correct: 0,
        learned: false,
        level: 1,
        listenedWords: 0,
        drawingAttempts: 0,
      }
      const nextPractices = oldProgress.practices + 1
      const nextCorrect = oldProgress.correct + Number(wasCorrect)
      const nextLevel = getProgressLevel(nextPractices, oldProgress.drawingAttempts, oldProgress.listenedWords)
      const totalCards = getLetterReadingCards(letters.find((item) => item.uppercase === letter) ?? selectedLetter).length
      const nextLearned = markLearned || hasLetterMastery({ ...oldProgress, practices: nextPractices, correct: nextCorrect, level: nextLevel }, totalCards)

      return {
        ...current,
        progress: {
          ...current.progress,
          [profileId]: {
            ...profileProgressData,
            [letter]: {
              practices: nextPractices,
              correct: nextCorrect,
              learned: nextLearned,
              level: nextLevel,
              listenedWords: oldProgress.listenedWords,
              drawingAttempts: oldProgress.drawingAttempts,
            },
          },
        },
        history: {
          ...current.history,
          [profileId]: [entry, ...(current.history[profileId] ?? [])].slice(0, 100),
        },
      }
    })
  }

  function recordMathAnswer(topic: MathTopic, correct: boolean) {
    if (!selectedProfile) return
    const profileId = selectedProfile.id
    updateData((current) => {
      const oldProgress = current.mathProgress[profileId] ?? emptyMathProgress()
      const oldTopicProgress = oldProgress.topics[topic]
      return {
        ...current,
        mathProgress: {
          ...current.mathProgress,
          [profileId]: {
            attempts: oldProgress.attempts + 1,
            correct: oldProgress.correct + Number(correct),
            topics: {
              ...oldProgress.topics,
              [topic]: {
                attempts: oldTopicProgress.attempts + 1,
                correct: oldTopicProgress.correct + Number(correct),
              },
            },
          },
        },
      }
    })
  }

  function markLearned() {
    if (!selectedProfile) return
    const profileId = selectedProfile.id
    updateData((current) => {
      const profileProgressData = current.progress[profileId] ?? {}
      const oldProgress = profileProgressData[selectedLetter.uppercase] ?? {
        practices: 0,
        correct: 0,
        learned: false,
        level: 1,
        listenedWords: 0,
        drawingAttempts: 0,
      }
      return {
        ...current,
        progress: {
          ...current.progress,
          [profileId]: {
            ...profileProgressData,
            [selectedLetter.uppercase]: {
              ...oldProgress,
              learned: true,
              level: Math.max(oldProgress.level, 3),
              listenedWords: Math.max(oldProgress.listenedWords, getLetterReadingCards(selectedLetter).length),
            },
          },
        },
      }
    })
    setFeedbackMessage(`Brawo! ${selectedProfile.name} poznał(a) literę ${selectedLetter.uppercase}.`, 'success')
  }

  async function speakText(text: string) {
    try {
      await playSpeech(text)
    } catch (error) {
      console.error('Nie udało się odtworzyć nagrania lektora.', error)
      setFeedbackMessage('Nie udało się odtworzyć nagrania. Odśwież aplikację i spróbuj ponownie.', 'error')
    }
  }

  async function speakLetter() {
    const cards = getLetterReadingCards(selectedLetter)
    for (const card of cards) {
      await speakText(`${card.phrase}.`)
    }
  }

  function markWordCardHeard(word: string) {
    if (!selectedProfile) return
    const totalCards = getLetterReadingCards(selectedLetter).length
    const profileId = selectedProfile.id
    updateData((current) => {
      const profileProgressData = current.progress[profileId] ?? {}
      const oldProgress = profileProgressData[selectedLetter.uppercase] ?? {
        practices: 0,
        correct: 0,
        learned: false,
        level: 1,
        listenedWords: 0,
        drawingAttempts: 0,
      }
      const nextHeard = Math.min(totalCards, oldProgress.listenedWords + 1)
      const nextLevel = getProgressLevel(oldProgress.practices, oldProgress.drawingAttempts, nextHeard)
      const shouldLearn = hasLetterMastery({ ...oldProgress, listenedWords: nextHeard, level: nextLevel }, totalCards)
      return {
        ...current,
        progress: {
          ...current.progress,
          [profileId]: {
            ...profileProgressData,
            [selectedLetter.uppercase]: {
              ...oldProgress,
              listenedWords: nextHeard,
              learned: oldProgress.learned || shouldLearn,
              level: nextLevel,
            },
          },
        },
      }
    })
    speakText(`${selectedLetter.uppercase} jak ${word}.`)
  }

  function startQuestion(type = exerciseType) {
    if (nextQuestionTimeoutRef.current !== null) {
      window.clearTimeout(nextQuestionTimeoutRef.current)
      nextQuestionTimeoutRef.current = null
    }
    answerInProgressRef.current = false
    setExerciseType(type)
    const previouslyAsked = askedLettersRef.current[type] ?? []
    const eligibleLetters = getEligibleLetters(type)
    const completedCycle = eligibleLetters.every((item) => previouslyAsked.includes(item.uppercase))
    const nextQuestion = createQuestion(type, previouslyAsked)
    askedLettersRef.current[type] = completedCycle
      ? [nextQuestion.letter.uppercase]
      : [...previouslyAsked, nextQuestion.letter.uppercase]
    setQuestion(nextQuestion)
    setAnswered(null)
    setFeedbackMessage('')

    window.setTimeout(() => {
      void speakText(getQuestionSpeech(nextQuestion))
    }, 50)
  }

  function answerQuestion(answer: string) {
    if (!currentQuestion || answerInProgressRef.current || !selectedProfile) return
    answerInProgressRef.current = true
    const normalizedAnswer = answer.trim().toLowerCase()
    const normalizedCorrect = currentQuestion.answer.trim().toLowerCase()
    const correct = normalizedAnswer === normalizedCorrect
    setAnswered(answer)
    setFeedbackMessage(
      correct ? 'Świetnie! Przechodzimy do kolejnego pytania.' : 'Spróbuj jeszcze raz.',
      correct ? 'success' : 'error',
    )
    recordPractice(
      currentQuestion.letter.uppercase,
      exerciseNames[currentQuestion.type],
      correct,
    )
    if (correct) {
      nextQuestionTimeoutRef.current = window.setTimeout(() => {
        nextQuestionTimeoutRef.current = null
        startQuestion(currentQuestion.type)
      }, 1200)
      return
    }

    void playSpeech('Spróbuj jeszcze raz.')
      .then(() => {
        answerInProgressRef.current = false
        setAnswered(null)
      })
      .catch((error: unknown) => {
        console.error('Nie udało się odtworzyć głosowej podpowiedzi.', error)
        answerInProgressRef.current = false
        setAnswered(null)
        setFeedbackMessage('Spróbuj jeszcze raz. Nie udało się odtworzyć podpowiedzi.', 'error')
      })
  }

  function switchRole() {
    setRole((current) => (current === 'Dziecko' ? 'Rodzic' : 'Dziecko'))
    startQuestion()
  }

  async function speakSyllableWord(word: string, wordSyllables: string[]) {
    if (syllableSpeechInProgressRef.current) return
    syllableSpeechInProgressRef.current = true
    setIsSpeakingSyllables(true)
    try {
      await playSpeech(`${word}. Słowo dzielimy na sylaby: ${wordSyllables.join(', ')}.`)
    } catch (error) {
      console.error('Nie udało się odczytać słowa z podziałem na sylaby.', error)
      setFeedbackMessage('Nie udało się odczytać sylab. Spróbuj ponownie.', 'error')
    } finally {
      syllableSpeechInProgressRef.current = false
      setIsSpeakingSyllables(false)
    }
  }

  const handleQuestionAnswer = useEffectEvent(answerQuestion)

  useEffect(() => {
    if (!currentQuestion || currentQuestion.type !== 'keyboard' || answered) return

    const handleKeyboardInput = (event: KeyboardEvent) => {
      const key = event.key
      if (!key || key.length !== 1) return

      if (event.key === 'Escape') return
      event.preventDefault()
      handleQuestionAnswer(key)
    }

    window.addEventListener('keydown', handleKeyboardInput)
    return () => window.removeEventListener('keydown', handleKeyboardInput)
  }, [currentQuestion, answered])

  function recordDrawing(isOnTarget: boolean) {
    if (!isOnTarget) {
      setFeedbackMessage('Spróbuj jeszcze raz — poprowadź palec bliżej kształtu litery.', 'error')
      return
    }
    if (!selectedProfile) return
    const profileId = selectedProfile.id
    updateData((current) => {
      const profileProgressData = current.progress[profileId] ?? {}
      const oldProgress = profileProgressData[selectedLetter.uppercase] ?? {
        practices: 0,
        correct: 0,
        learned: false,
        level: 1,
        listenedWords: 0,
        drawingAttempts: 0,
      }
      const nextAttempts = oldProgress.drawingAttempts + 1
      const totalCards = getLetterReadingCards(selectedLetter).length
      const nextLevel = getProgressLevel(oldProgress.practices, nextAttempts, oldProgress.listenedWords)
      const learned = oldProgress.learned || hasLetterMastery({ ...oldProgress, drawingAttempts: nextAttempts, level: nextLevel }, totalCards)
      return {
        ...current,
        progress: {
          ...current.progress,
          [profileId]: {
            ...profileProgressData,
            [selectedLetter.uppercase]: {
              ...oldProgress,
              learned,
              level: nextLevel,
              drawingAttempts: nextAttempts,
            },
          },
        },
      }
    })
    recordPractice(
      selectedLetter.uppercase,
      `Rysowanie litery ${drawVariant === 'lowercase' ? 'małą' : 'dużą'} literą`,
      true,
    )
    setFeedbackMessage('Pięknie ćwiczysz! Każdy narysowany kształt to kolejna próba.', 'success')
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <a className="brand" href="#" aria-label="Uczę się z Tobą — strona główna">
          <span className="brand-mark">🌱</span>
          <span>
            <strong>Uczę się z Tobą</strong>
            <small>Małe odkrycia, wielka radość</small>
          </span>
        </a>
        <nav className="main-nav" aria-label="Główna nawigacja">
          <button className={activeTab === 'discover' ? 'nav-item active' : 'nav-item'} onClick={() => setActiveTab('discover')}>
            <span>🔤</span> Litery
          </button>
          <button className={activeTab === 'syllables' ? 'nav-item active' : 'nav-item'} onClick={() => setActiveTab('syllables')}>
            <span>🎵</span> Sylaby
          </button>
          <button className={activeTab === 'math' ? 'nav-item active' : 'nav-item'} onClick={() => setActiveTab('math')}>
            <span>🔢</span> Matematyka
          </button>
          <button className={activeTab === 'practice' ? 'nav-item active' : 'nav-item'} onClick={() => setActiveTab('practice')}>
            <span>🎲</span> Zabawy
          </button>
          <button className={activeTab === 'progress' ? 'nav-item active' : 'nav-item'} onClick={() => setActiveTab('progress')}>
            <span>🌟</span> Postępy
          </button>
        </nav>
        <div className="offline-pill"><span /> Lektor Gosia · offline</div>
      </header>

      <div className="layout">
        <aside className="sidebar">
          <section className="profile-card card">
            <div className="section-heading">
              <div>
                <span className="eyebrow">NASZA DRUŻYNA</span>
                <h2>Kto dziś odkrywa?</h2>
              </div>
              <span className="heading-icon">👋</span>
            </div>
            {data.profiles.length > 0 ? (
              <div className="profile-list">
                {data.profiles.map((profile) => (
                  <div className="profile-row" key={profile.id}>
                    <button
                      className={profile.id === selectedProfile?.id ? 'profile-option selected' : 'profile-option'}
                      onClick={() => selectProfile(profile.id)}
                    >
                      <span className="avatar">{profile.avatar}</span>
                      <span className="profile-name">{profile.name}</span>
                      {profile.id === selectedProfile?.id && <span className="check-mark">✓</span>}
                    </button>
                    <button
                      className="profile-delete"
                      type="button"
                      aria-label={`Usuń profil ${profile.name}`}
                      onClick={() => deleteProfile(profile)}
                    >
                      Usuń
                    </button>
                  </div>
                ))}
              </div>
            ) : (
              <p className="muted small">Dodaj profil, żeby zacząć wspólną naukę.</p>
            )}
            <form className="add-profile-form" onSubmit={addProfile}>
              <label htmlFor="child-name">Dodaj małego odkrywcę</label>
              <input
                id="child-name"
                maxLength={30}
                placeholder="Imię dziecka"
                value={newName}
                onChange={(event) => setNewName(event.target.value)}
              />
              <div className="avatar-picker" aria-label="Wybierz awatar">
                {avatars.map((avatar) => (
                  <button
                    className={avatar === newAvatar ? 'avatar-choice chosen' : 'avatar-choice'}
                    key={avatar}
                    type="button"
                    aria-label={`Wybierz awatar ${avatar}`}
                    onClick={() => setNewAvatar(avatar)}
                  >
                    {avatar}
                  </button>
                ))}
              </div>
              {formError && <p className="error-message">{formError}</p>}
              <button className="button button-outline add-button" type="submit">
                <span>＋</span> Dodaj profil
              </button>
            </form>
            <div className="privacy-note"><span>🔒</span> Profile i postępy zostają w tej przeglądarce.</div>
          </section>

          <section className="daily-card">
            <span className="daily-illustration">☀️</span>
            <div>
              <strong>Każdy dzień to nowa przygoda!</strong>
              <p>Uczymy się razem, we własnym tempie.</p>
            </div>
          </section>
        </aside>

        <main className="main-content">
          {storageError && <div className="storage-warning" role="status">{storageError}</div>}

          <section className="welcome-row">
            <div>
              <span className="eyebrow">{isMathTab ? 'WITAJCIE W ŚWIECIE LICZB' : 'WITAJCIE W KRAINIE LITER'}</span>
              <h1>
                {selectedProfile ? `Cześć, ${selectedProfile.name}!` : 'Witajcie, odkrywcy!'}
              </h1>
              <p className="welcome-subtitle">
                {isMathTab
                  ? 'Odkrywajcie liczby razem — spokojnie i z uśmiechem.'
                  : 'Odkrywajcie litery razem — spokojnie i z uśmiechem.'}
              </p>
            </div>
            <div className="welcome-doodle" aria-hidden="true">
              <span>☁️</span><span>🌈</span><span>⭐</span>
            </div>
          </section>

          <section className="progress-banner card">
            <div className="progress-icon">{isMathTab ? '🔢' : '🌼'}</div>
            <div className="progress-info">
              <div className="progress-title-row">
                <strong>{isMathTab ? 'Wasza podróż przez cyfry' : 'Wasza podróż przez alfabet'}</strong>
                <span>{isMathTab
                  ? `${selectedMathProgress.correct} poprawnych odpowiedzi z ${selectedMathProgress.attempts} prób`
                  : `${learnedCount} z ${letters.length} liter`}</span>
              </div>
              <div className="progress-track">
                <div style={{ width: `${isMathTab ? mathProgressPercent : progressPercent}%` }} />
              </div>
              <p>{isMathTab ? mathBadge : badge}</p>
            </div>
            <div className="progress-percent">{isMathTab ? mathProgressPercent : progressPercent}<small>%</small></div>
          </section>

          <div className="tab-content">
            {activeTab === 'discover' && (
              <div className="discover-view">
                <section className="card alphabet-card">
                  <div className="section-heading">
                    <div>
                      <span className="eyebrow">POZNAJMY SIĘ</span>
                      <h2>Polski alfabet</h2>
                    </div>
                    <span className="heading-icon">🧩</span>
                  </div>
                  <p className="muted">Wybierz literę, zobacz przykład i spróbuj ją narysować.</p>
                  <div className="alphabet-grid" aria-label="Polski alfabet">
                    {letters.map((letter) => {
                      const progress = profileProgress[letter.uppercase]
                      const isLetterKnown = Boolean(progress?.learned || progress?.level >= 3)
                      return (
                        <button
                          key={letter.uppercase}
                          className={[
                            'letter-tile',
                            selectedLetter.uppercase === letter.uppercase ? 'active' : '',
                            isLetterKnown ? 'learned' : '',
                          ].join(' ')}
                          onClick={() => {
                            setSelectedLetter(letter)
                            setFeedback('')
                          }}
                          aria-label={`${letter.uppercase}, ${letter.lowercase}${isLetterKnown ? ', poznana' : ''}`}
                          aria-pressed={selectedLetter.uppercase === letter.uppercase}
                        >
                          <span>{letter.uppercase}</span>
                          {isLetterKnown && <i>✓</i>}
                        </button>
                      )
                    })}
                  </div>
                </section>

                <section className="card letter-detail">
                  <div className="letter-showcase">
                    <span className="big-letter">{selectedLetter.uppercase}</span>
                    <span className="small-letter">{selectedLetter.lowercase}</span>
                    <span className="letter-sparkle">✦</span>
                  </div>
                  <div className="letter-info">
                    <span className="eyebrow">POZNAJ LITERĘ</span>
                    <h2>{selectedLetter.uppercase} jak {selectedLetter.word}</h2>
                    <div className="level-row">
                      <span className="level-label">Poziom {selectedLetterProgress?.level ?? 1}/5</span>
                      <span className="level-stars" aria-label={`Poziom ${selectedLetterProgress?.level ?? 1} z 5`}>
                        {getStars(selectedLetterProgress?.level ?? 1)}
                      </span>
                    </div>
                    <p className="muted">Posłuchaj, powtórz i poznaj nowe słowo.</p>
                    <div className="example-word">
                      <span className="example-emoji">{selectedLetter.illustration}</span>
                      <span><strong>{selectedLetter.word}</strong><small>zaczyna się na literę {selectedLetter.uppercase}</small></span>
                    </div>
                    <div className="reading-card-list">
                      {getLetterReadingCards(selectedLetter).map((card, index) => {
                        const isHeard = index < (selectedLetterProgress?.listenedWords ?? 0)
                        return (
                          <button
                            key={card.id}
                            type="button"
                            className={isHeard ? 'reading-card heard' : 'reading-card'}
                            onClick={() => markWordCardHeard(card.word)}
                          >
                            <span className="reading-card-icon">{isHeard ? '✓' : '🔊'}</span>
                            <strong>{card.phrase}</strong>
                          </button>
                        )
                      })}
                    </div>
                    <div className="progress-meta">
                      <span>Usłyszane karty: {Math.min(selectedLetterProgress?.listenedWords ?? 0, getLetterReadingCards(selectedLetter).length)} / {getLetterReadingCards(selectedLetter).length}</span>
                    </div>
                    <div className="letter-actions">
                      <button className="button button-soft" onClick={speakLetter}><span>🔊</span> Posłuchaj wszystko</button>
                      <button
                        className="button button-primary"
                        disabled={
                          !selectedProfile ||
                          (selectedLetterProgress?.listenedWords ?? 0) < getLetterReadingCards(selectedLetter).length ||
                          (selectedLetterProgress?.drawingAttempts ?? 0) < 10
                        }
                        onClick={markLearned}
                      >
                        <span>{(selectedLetterProgress?.listenedWords ?? 0) >= getLetterReadingCards(selectedLetter).length && (selectedLetterProgress?.drawingAttempts ?? 0) >= 10 ? '✓' : '⭐'}</span>
                        {(selectedLetterProgress?.listenedWords ?? 0) >= getLetterReadingCards(selectedLetter).length && (selectedLetterProgress?.drawingAttempts ?? 0) >= 10 ? 'Zaliczone!' : 'Zalicz literę'}
                      </button>
                    </div>
                    {feedback && (
                      <p
                        className={['feedback-message', feedbackTone === 'success' ? 'is-success' : '', feedbackTone === 'error' ? 'is-error' : ''].join(' ')}
                        role="status"
                      >
                        {feedback}
                      </p>
                    )}
                  </div>
                </section>

                <section className="card drawing-card">
                  <div className="section-heading">
                    <div>
                      <span className="eyebrow">ĆWICZYMY RAZEM</span>
                      <h2>Narysuj literę palcem</h2>
                    </div>
                    <span className="heading-icon">✍️</span>
                  </div>
                  <p className="muted">Przesuwaj palcem po polu — jasna litera jest podpowiedzią.</p>
                  <div className="drawing-launcher">
                    <span className="drawing-launcher-letter">{selectedLetter.uppercase}</span>
                    <div className="drawing-launcher-copy">
                      <strong>Gotowi na rysowanie?</strong>
                      <small>Otwórz duże pole i prześledź kształt palcem.</small>
                    </div>
                    <button
                      ref={drawingTriggerRef}
                      className="button button-primary"
                      type="button"
                      onClick={() => setIsDrawingOpen(true)}
                    >
                      ✍️ Rysujemy
                    </button>
                  </div>
                  <div className="drawing-progress-row">
                    <span>Rysowania: {selectedLetterProgress?.drawingAttempts ?? 0} / 10</span>
                  </div>
                  {feedback && (
                    <p
                      className={['feedback-message', feedbackTone === 'success' ? 'is-success' : '', feedbackTone === 'error' ? 'is-error' : ''].join(' ')}
                      role="status"
                    >
                      {feedback}
                    </p>
                  )}
                </section>
              </div>
            )}

            {activeTab === 'syllables' && (
              <section className="card syllables-card">
                <div className="section-heading">
                  <div>
                    <span className="eyebrow">DRUGA STREFA</span>
                    <h2>Strefa sylab</h2>
                  </div>
                  <span className="heading-icon">🎵</span>
                </div>
                <p className="muted">Słysz, czytaj sylaby i ucz się od prostych do trudniejszych wyrazów.</p>
                <div className="syllable-filters">
                  <label>
                    Kategoria
                    <select value={syllableCategory} onChange={(event) => setSyllableCategory(event.target.value)}>
                      <option value="all">Wszystkie</option>
                      {syllableWordCategories.map((category) => (
                        <option key={category} value={category}>{category}</option>
                      ))}
                    </select>
                  </label>
                  <label>
                    Liczba sylab
                    <select value={syllableLength} onChange={(event) => setSyllableLength(event.target.value === 'all' ? 'all' : Number(event.target.value))}>
                      <option value="all">Wszystkie</option>
                      {[1, 2, 3, 4, 5, 6].map((length) => (
                        <option key={length} value={length}>{length} sylaby</option>
                      ))}
                    </select>
                  </label>
                </div>
                <div className="syllable-grid">
                  {visibleSyllableWords.slice(0, 30).map((item) => (
                    <button
                      key={item.id}
                      className="syllable-card"
                      type="button"
                      disabled={isSpeakingSyllables}
                      onClick={() => void speakSyllableWord(item.word, item.syllables)}
                    >
                      <span className="syllable-icon">{item.category === 'zwierzęta' ? '🐾' : item.category === 'jedzenie' ? '🍎' : item.category === 'dom' ? '🏠' : item.category === 'przyroda' ? '🌼' : '📚'}</span>
                      <strong>{item.word}</strong>
                      <small>{item.syllables.join('  ·  ')}</small>
                      <span className="syllable-badge">{item.syllableCount} sylaby</span>
                    </button>
                  ))}
                </div>
                {feedback && (
                  <p className={`feedback-message ${feedbackTone === 'success' ? 'is-success' : ''} ${feedbackTone === 'error' ? 'is-error' : ''}`} role="status">
                    {feedback}
                  </p>
                )}
              </section>
            )}

            {activeTab === 'math' && (
              <MathZone
                profileName={selectedProfile?.name}
                progress={selectedMathProgress}
                onAnswer={recordMathAnswer}
              />
            )}

            {activeTab === 'practice' && (
              <section className="card exercise-card">
                <div className="section-heading">
                  <div>
                    <span className="eyebrow">CZAS NA ZABAWĘ</span>
                    <h2>Wspólne zgadywanki</h2>
                  </div>
                  <span className="heading-icon">🎲</span>
                </div>
                <p className="muted">Nie ma złych odpowiedzi — każda próba pomaga się uczyć.</p>
                <div className="role-banner">
                  <span>{role === 'Dziecko' ? '🧒' : '🧑‍🦱'}</span>
                  <p>{role === 'Dziecko' ? `${selectedProfile?.name ?? 'Odkrywco'}, teraz Twoja kolej. Rodzic może Ci pomóc!` : 'Teraz Ty uczysz rodzica. Wybierzcie odpowiedź razem!'}</p>
                </div>
                <div className="exercise-controls">
                  <label htmlFor="exercise-type">Wybierz zabawę</label>
                  <select
                    id="exercise-type"
                    value={exerciseType}
                    onChange={(event) => startQuestion(event.target.value as ExerciseType)}
                  >
                    {Object.entries(exerciseNames).map(([type, name]) => (
                      <option key={type} value={type}>{name}</option>
                    ))}
                  </select>
                  <button className="button button-primary" onClick={() => startQuestion()}>
                    {currentQuestion ? '🎲 Nowe pytanie' : '✨ Zaczynamy!'}
                  </button>
                  <button className="button button-soft" onClick={switchRole}>🔄 Zamień role</button>
                </div>
                {currentQuestion ? (
                  <div className="question-area">
                    <div className="question-illustration">{currentQuestion.letter.illustration}</div>
                    <h3>{currentQuestion.prompt}</h3>
                    {currentQuestion.type === 'keyboard' ? (
                      <div className="keyboard-challenge">
                        <div className="keyboard-target" aria-label="Odgadnij usłyszaną literę">?</div>
                        <p className="muted">Posłuchaj i naciśnij odpowiednią literę na klawiaturze.</p>
                        <button className="button button-soft" type="button" onClick={() => speakText(`Litera ${currentQuestion.answer}`)}>
                          🔊 Odczytaj jeszcze raz
                        </button>
                      </div>
                    ) : (
                      <div className={`answer-options ${currentQuestion.type === 'match-word' ? 'picture-options' : ''}`}>
                        {currentQuestion.options.map((option, index) => {
                          const isCorrect = option === currentQuestion.answer
                          const isSelected = option === answered
                          return (
                            <button
                              key={`${option}-${index}`}
                              className={[
                                'answer-option',
                                currentQuestion.type === 'match-word' ? 'picture-option' : '',
                                answered && isCorrect ? 'correct' : '',
                                isSelected && !isCorrect ? 'incorrect' : '',
                              ].join(' ')}
                              disabled={Boolean(answered) || !selectedProfile}
                              onClick={() => answerQuestion(option)}
                            >
                              {option}
                            </button>
                          )
                        })}
                      </div>
                    )}
                    {feedback && <p className={`answer-feedback ${isCurrentAnswerCorrect ? 'positive' : 'gentle'}`} role="status">{feedback}</p>}
                  </div>
                ) : (
                  <div className="empty-question">
                    <span>🐝</span>
                    <p>Gotowi na małą zagadkę?</p>
                    <small>Naciśnij „Zaczynamy!” i wybierzcie odpowiedź razem.</small>
                  </div>
                )}
              </section>
            )}

            {activeTab === 'progress' && (
              <div className="progress-view">
                <section className="card stats-card">
                  <div className="section-heading">
                    <div>
                      <span className="eyebrow">JESTEŚMY Z SIEBIE DUMNI</span>
                      <h2>Nasze małe wielkie kroki</h2>
                    </div>
                    <span className="heading-icon">🏆</span>
                  </div>
                  <div className="stats-grid">
                    <div className="stat-box"><span>🌟</span><strong>{learnedCount}</strong><small>Poznane litery</small></div>
                    <div className="stat-box"><span>🎯</span><strong>{practiceCount}</strong><small>Wspólne próby</small></div>
                    <div className="stat-box"><span>📚</span><strong>{letters.length - learnedCount}</strong><small>Liter czeka na odkrycie</small></div>
                  </div>
                  <div className="learned-list">
                    {letters.map((letter) => {
                      const progress = profileProgress[letter.uppercase]
                      const learned = Boolean(progress?.learned || progress?.level >= 3)
                      return (
                        <button
                          key={letter.uppercase}
                          className={learned ? 'progress-letter learned' : 'progress-letter'}
                          onClick={() => {
                            setSelectedLetter(letter)
                            setActiveTab('discover')
                          }}
                          title={learned ? `${letter.uppercase} — poznana` : `${letter.uppercase} — jeszcze niepoznana`}
                        >
                          {learned ? '✓' : '·'} {letter.uppercase}
                        </button>
                      )
                    })}
                  </div>
                </section>

                <section className="card history-card">
                  <div className="section-heading">
                    <div>
                      <span className="eyebrow">ZAPISANE CHWILE</span>
                      <h2>Ostatnie aktywności</h2>
                    </div>
                    <span className="heading-icon">📖</span>
                  </div>
                  {history.length ? (
                    <ul className="history-list">
                      {history.slice(0, 12).map((entry) => (
                        <li key={entry.id}>
                          <span className={entry.wasCorrect ? 'history-check good' : 'history-check'}>{entry.wasCorrect ? '✓' : '↻'}</span>
                          <span className="history-copy"><strong>{entry.letter} · {entry.description}</strong><small>{entry.displayAt}</small></span>
                          <span className="history-result">{entry.wasCorrect ? 'Brawo!' : 'Próba'}</span>
                        </li>
                      ))}
                    </ul>
                  ) : (
                    <div className="empty-history"><span>📚</span><p>Tu pojawią się Wasze wspólne odkrycia.</p></div>
                  )}
                </section>
              </div>
            )}
          </div>
          <footer className="page-footer">Stworzone z ciekawością i czułością <span>♥</span></footer>
        </main>
      </div>
      {isDrawingOpen && (
        <div
          className="drawing-modal-backdrop"
          onMouseDown={(event) => {
            if (event.target === event.currentTarget) setIsDrawingOpen(false)
          }}
        >
          <section
            className="drawing-modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="drawing-modal-title"
          >
            <header className="drawing-modal-header">
              <div>
                <span className="eyebrow">ĆWICZYMY RAZEM</span>
                <h2 id="drawing-modal-title">Narysuj literę {selectedLetter.uppercase}</h2>
                <p>Przesuwaj palcem po jasnej podpowiedzi.</p>
              </div>
              <button
                ref={closeDrawingRef}
                className="modal-close-button"
                type="button"
                aria-label="Zamknij okno rysowania"
                onClick={() => setIsDrawingOpen(false)}
              >
                ×
              </button>
            </header>
            <div className="draw-mode-toggle" aria-label="Wybierz wariant litery do narysowania">
              <button
                className={drawVariant === 'uppercase' ? 'draw-mode-button active' : 'draw-mode-button'}
                type="button"
                onClick={() => setDrawVariant('uppercase')}
              >
                Duża {selectedLetter.uppercase}
              </button>
              <button
                className={drawVariant === 'lowercase' ? 'draw-mode-button active' : 'draw-mode-button'}
                type="button"
                onClick={() => setDrawVariant('lowercase')}
              >
                Mała {selectedLetter.lowercase}
              </button>
            </div>
            <DrawingPad letter={selectedLetter.uppercase} variant={drawVariant} onComplete={recordDrawing} large />
            <footer className="drawing-modal-footer">
              <p>Nie musi być idealnie — liczy się każda próba! 🌱</p>
              <button className="button button-soft" type="button" onClick={() => setIsDrawingOpen(false)}>
                Gotowe
              </button>
            </footer>
          </section>
        </div>
      )}
    </div>
  )
}

export default App
