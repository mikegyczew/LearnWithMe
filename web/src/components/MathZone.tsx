import { useCallback, useEffect, useState } from 'react'
import { DrawingPad } from './DrawingPad'
import type { MathProgress, MathTopic } from './mathProgress'
import { playSpeech } from '../speech'

type MathQuestion = {
  prompt: string
  answer: number
  choices: number[]
  kind: MathTopic
  first?: number
  second?: number
  groups?: number
  perGroup?: number
}

type MathZoneProps = {
  profileName?: string
  progress: MathProgress
  onAnswer: (topic: MathTopic, correct: boolean) => void
}

const topicLabels: Record<MathTopic, string> = {
  numbers: 'Cyfry i liczby',
  counting: 'Liczymy',
  addition: 'Dodawanie',
  subtraction: 'Odejmowanie',
  multiplication: 'Równe grupy',
  division: 'Dzielenie się',
}

const topicDescriptions: Record<MathTopic, string> = {
  numbers: 'Poznaj cyfry i liczby od 0 do 20.',
  counting: 'Policz obrazki i wybierz właściwą liczbę.',
  addition: 'Dodajemy razem — do 20.',
  subtraction: 'Odejmujemy spokojnie — w zakresie 20.',
  multiplication: 'Mnożenie to równe grupy takich samych rzeczy.',
  division: 'Dzielimy po równo, aby każdy dostał tyle samo.',
}

const topicEmojis: Record<MathTopic, string> = {
  numbers: '🔢',
  counting: '🐞',
  addition: '➕',
  subtraction: '➖',
  multiplication: '🍎',
  division: '🧺',
}

const objects = ['🍎', '⭐', '🐻', '🍓', '🚗']
const numberWords = ['zero', 'jeden', 'dwa', 'trzy', 'cztery', 'pięć', 'sześć', 'siedem', 'osiem', 'dziewięć', 'dziesięć']
const teenWords = ['jedenaście', 'dwanaście', 'trzynaście', 'czternaście', 'piętnaście', 'szesnaście', 'siedemnaście', 'osiemnaście', 'dziewiętnaście', 'dwadzieścia']
const numberExamples = [
  { icon: '🚗', one: 'jeden samochód', two: 'dwa', few: 'samochody', many: 'samochodów' },
  { icon: '👗', one: 'jedna sukienka', two: 'dwie', few: 'sukienki', many: 'sukienek' },
  { icon: '🧸', one: 'jedna zabawka', two: 'dwie', few: 'zabawki', many: 'zabawek' },
]

function getNumberWord(value: number) {
  return value <= 10 ? numberWords[value] : teenWords[value - 11]
}

function getCountPhrase(value: number, item: typeof numberExamples[number]) {
  if (value === 1) return item.one
  const lastTwo = value % 100
  const last = value % 10
  const form = last >= 2 && last <= 4 && (lastTwo < 12 || lastTwo > 14) ? item.few : item.many
  const number = value === 2 ? item.two : getNumberWord(value)
  return `${number} ${form}`
}

function randomInt(min: number, max: number) {
  return Math.floor(Math.random() * (max - min + 1)) + min
}

function getChoices(answer: number, max = 20) {
  const choices = new Set([answer])
  while (choices.size < 4) {
    const offset = randomInt(-4, 4)
    const choice = Math.max(0, Math.min(max, answer + (offset === 0 ? 1 : offset)))
    choices.add(choice)
  }
  return [...choices].sort(() => Math.random() - 0.5)
}

function makeQuestion(topic: MathTopic): MathQuestion {
  switch (topic) {
    case 'numbers': {
      const answer = randomInt(0, 20)
      return { kind: topic, prompt: `Znajdź liczbę ${answer}.`, answer, choices: getChoices(answer) }
    }
    case 'counting': {
      const answer = randomInt(1, 10)
      return { kind: topic, prompt: 'Policz obrazki. Ile ich jest?', answer, choices: getChoices(answer, 10) }
    }
    case 'addition': {
      const first = randomInt(0, 10)
      const second = randomInt(0, 20 - first)
      return {
        kind: topic,
        prompt: `Ile to ${first} + ${second}?`,
        first,
        second,
        answer: first + second,
        choices: getChoices(first + second),
      }
    }
    case 'subtraction': {
      const first = randomInt(1, 20)
      const second = randomInt(0, first)
      return {
        kind: topic,
        prompt: `Ile to ${first} − ${second}?`,
        first,
        second,
        answer: first - second,
        choices: getChoices(first - second),
      }
    }
    case 'multiplication': {
      const groups = randomInt(2, 5)
      const perGroup = randomInt(2, Math.min(5, Math.floor(20 / groups)))
      return {
        kind: topic,
        prompt: `Są ${groups} równe grupy. W każdej są ${perGroup}. Ile jest razem?`,
        groups,
        perGroup,
        answer: groups * perGroup,
        choices: getChoices(groups * perGroup),
      }
    }
    case 'division': {
      const groups = randomInt(2, 5)
      const perGroup = randomInt(1, Math.min(5, Math.floor(20 / groups)))
      return {
        kind: topic,
        prompt: `Podziel ${groups * perGroup} po równo do ${groups} koszyków. Ile będzie w każdym?`,
        groups,
        perGroup,
        answer: perGroup,
        choices: getChoices(perGroup, 10),
      }
    }
  }
}

function MathVisual({ question }: { question: MathQuestion }) {
  const emoji = objects[question.answer % objects.length]
  if (question.kind === 'numbers') {
    return null
  }
  if (question.kind === 'counting') {
    return (
      <div className="math-count-objects" aria-label={`${question.answer} obrazków`}>
        {Array.from({ length: question.answer }, (_, index) => <span key={index}>{emoji}</span>)}
      </div>
    )
  }
  if (question.kind === 'addition' || question.kind === 'subtraction') {
    const visible = question.kind === 'addition'
      ? [...Array.from({ length: question.first ?? 0 }), ...Array.from({ length: question.second ?? 0 })]
      : Array.from({ length: question.first ?? 0 }, (_, index) => index < (question.second ?? 0))
    return (
      <div className="math-count-objects" aria-label="Obrazki pomagają policzyć działanie">
        {visible.map((removed, index) => (
          <span className={removed ? 'math-object-removed' : ''} key={index}>{emoji}</span>
        ))}
      </div>
    )
  }
  return (
    <div className="math-groups-visual" aria-label={`${question.groups} równych grup po ${question.perGroup}`}>
      {Array.from({ length: question.groups ?? 0 }, (_, group) => (
        <div className="math-group" key={group}>
          <span className="math-group-label">{question.kind === 'division' ? `Koszyk ${group + 1}` : `Grupa ${group + 1}`}</span>
          <div>{Array.from({ length: question.perGroup ?? 0 }, (_, item) => <span key={item}>{emoji}</span>)}</div>
        </div>
      ))}
    </div>
  )
}

function NumberJourney({
  question,
  onTrace,
  onAnswer,
  profileName,
  answered,
  isCorrect,
  feedback,
  onSpeak,
}: {
  question: MathQuestion
  onTrace: (wasCorrect: boolean) => void
  onAnswer: (value: number) => void
  profileName?: string
  answered: number | null
  isCorrect: boolean | null
  feedback: string
  onSpeak: (text: string) => void
}) {
  return (
    <div className="number-journey">
      <header className="number-journey-heading">
        <span className="number-journey-mark">🔢</span>
        <div>
          <span className="eyebrow">POZNAJEMY CYFRY RAZEM</span>
          <h3>Wasza podróż przez Cyfry</h3>
          <p>Popatrz, przeczytaj na głos, a potem spróbuj narysować.</p>
        </div>
      </header>
      <div className="number-learning-card">
        <div className="number-face" aria-label={`Liczba ${question.answer}`}>{question.answer}</div>
        <div className="number-pronunciation">
          <span>Ta liczba to</span>
          <strong>{getNumberWord(question.answer)}</strong>
          <button
            className="button button-soft"
            type="button"
            onClick={() => onSpeak(`${question.prompt} ${getNumberWord(question.answer)}. ${numberExamples.map((item) => getCountPhrase(question.answer, item)).join('. ')}`)}
          >
            🔊 Posłuchaj
          </button>
        </div>
      </div>
      <div className="number-task">
        <h4>{question.prompt}</h4>
        <div className="math-answer-options" aria-label="Wybierz zapis tej liczby">
          {question.choices.map((choice) => (
            <button
              className={[
                'math-answer-option',
                answered === question.answer && choice === question.answer ? 'correct' : '',
                answered === choice && choice !== question.answer ? 'incorrect' : '',
              ].join(' ')}
              type="button"
              key={choice}
              disabled={!profileName || answered !== null}
              onClick={() => onAnswer(choice)}
            >
              {choice}
            </button>
          ))}
        </div>
        {feedback && (
          <p className={`math-feedback ${isCorrect ? 'positive' : 'gentle'}`} role="status">{feedback}</p>
        )}
        {!profileName && <p className="math-profile-reminder">Dodaj lub wybierz profil, aby zapisywać postępy.</p>}
      </div>
      <div className="number-reading-examples" aria-label="Czytamy liczby z nazwami rzeczy">
        {numberExamples.map((item) => (
          <button
            className="number-reading-example"
            type="button"
            key={item.one}
            onClick={() => onSpeak(getCountPhrase(question.answer, item))}
          >
            <span>{item.icon}</span>
            <strong>{getCountPhrase(question.answer, item)}</strong>
            {question.answer !== 1 && <small>1: {item.one}</small>}
            <small>🔊 Posłuchaj</small>
          </button>
        ))}
      </div>
      <div className="number-tracing-card">
        <div>
          <h4>A teraz narysuj {getNumberWord(question.answer)}!</h4>
          <p>Poprowadź palec po jasnym śladzie.</p>
        </div>
        <DrawingPad
          letter={String(question.answer)}
          subjectLabel="liczby"
          onComplete={onTrace}
        />
      </div>
    </div>
  )
}

export function MathZone({ profileName, progress, onAnswer }: MathZoneProps) {
  const [topic, setTopic] = useState<MathTopic>('numbers')
  const [question, setQuestion] = useState(() => makeQuestion('numbers'))
  const [feedback, setFeedback] = useState('')
  const [isCorrect, setIsCorrect] = useState<boolean | null>(null)
  const [answered, setAnswered] = useState<number | null>(null)
  const [advanceOnTrace, setAdvanceOnTrace] = useState(false)
  const [audioError, setAudioError] = useState('')

  function speakPolish(text: string) {
    setAudioError('')
    void playSpeech(text).catch((error: unknown) => {
      console.error('Nie udało się odtworzyć głosu Gosia.', error)
      setAudioError('Nie udało się odtworzyć nagrania. Spróbuj ponownie.')
    })
  }

  const nextQuestion = useCallback(() => {
    setQuestion(makeQuestion(topic))
    setAnswered(null)
    setIsCorrect(null)
    setFeedback('')
    setAdvanceOnTrace(false)
  }, [topic])

  useEffect(() => {
    if (answered === null) return
    if (isCorrect && topic === 'numbers' && !advanceOnTrace) return
    const timeout = window.setTimeout(() => {
      if (isCorrect) nextQuestion()
      else setAnswered(null)
    }, 1400)
    return () => window.clearTimeout(timeout)
  }, [answered, isCorrect, topic, advanceOnTrace, nextQuestion])

  function startTopic(nextTopic: MathTopic) {
    setTopic(nextTopic)
    setQuestion(makeQuestion(nextTopic))
    setAnswered(null)
    setIsCorrect(null)
    setFeedback('')
    setAdvanceOnTrace(false)
  }

  function chooseAnswer(value: number) {
    if (answered !== null || !profileName) return
    const correct = value === question.answer
    setAnswered(correct ? question.answer : value)
    setIsCorrect(correct)
    setFeedback(
      correct
        ? topic === 'numbers'
          ? 'Dobra robota! Teraz przeczytaj i narysuj tę liczbę. 🌟'
          : 'Dobra robota! Za chwilę nowe zadanie. 🌟'
        : 'Spróbuj ponownie albo policz obrazki. Każda próba pomaga się uczyć. 💚',
    )
    speakPolish(correct ? 'Dobra robota!' : 'Spróbuj ponownie.')
    onAnswer(topic, correct)
  }

  function recordNumberTrace(wasCorrect: boolean) {
    if (!wasCorrect) {
      onAnswer('numbers', false)
      setFeedback('Spróbuj ponownie — prowadź palec po jasnym śladzie.')
      setIsCorrect(false)
      speakPolish('Spróbuj ponownie.')
      return
    }

    if (answered === null) chooseAnswer(question.answer)
    else onAnswer('numbers', true)
    setAnswered(question.answer)
    setIsCorrect(true)
    setFeedback('Dobra robota! Pięknie narysowana liczba.')
    setAdvanceOnTrace(true)
    if (answered !== null) speakPolish('Dobra robota!')
  }

  const currentTopicProgress = progress.topics[topic] ?? { attempts: 0, correct: 0 }

  return (
    <section className="card math-zone">
      <div className="section-heading">
        <div>
          <span className="eyebrow">NOWA PRZYGODA</span>
          <h2>Strefa matematyki</h2>
        </div>
        <span className="heading-icon">🔢</span>
      </div>
      <p className="muted math-intro">
        Cześć{profileName ? `, ${profileName}` : ''}! Uczymy się liczb i działań krok po kroku. Pomyłki pomagają nam się uczyć.
      </p>
      <div className="math-topic-list" aria-label="Wybierz zabawę matematyczną">
        {(Object.keys(topicLabels) as MathTopic[]).map((item) => (
          <button
            className={topic === item ? 'math-topic-button active' : 'math-topic-button'}
            type="button"
            key={item}
            aria-pressed={topic === item}
            onClick={() => startTopic(item)}
          >
            <span>{topicEmojis[item]}</span>{topicLabels[item]}
          </button>
        ))}
      </div>
      {topic === 'numbers' ? (
        <NumberJourney
          question={question}
          onTrace={recordNumberTrace}
          onAnswer={chooseAnswer}
          profileName={profileName}
          answered={answered}
          isCorrect={isCorrect}
          feedback={feedback}
          onSpeak={speakPolish}
        />
      ) : (
      <>
      <p className="math-topic-description">{topicDescriptions[topic]}</p>
      <div className="math-question-card">
        <div className="math-question-heading">
          <span>{topicEmojis[topic]}</span>
          <div>
            <h3>{question.prompt}</h3>
            <button className="button button-soft" type="button" onClick={() => speakPolish(question.prompt)}>
              🔊 Posłuchaj zadania
            </button>
          </div>
        </div>
        <MathVisual question={question} />
        <div className="math-answer-options" aria-label="Możliwe odpowiedzi">
          {question.choices.map((choice) => {
            const isAnswer = choice === question.answer
            const wasPicked = choice === answered
            return (
              <button
                className={[
                  'math-answer-option',
                  answered === question.answer && isAnswer ? 'correct' : '',
                  wasPicked && !isAnswer ? 'incorrect' : '',
                ].join(' ')}
                type="button"
                key={choice}
                disabled={!profileName || answered !== null}
                aria-pressed={wasPicked}
                onClick={() => chooseAnswer(choice)}
              >
                {choice}
              </button>
            )
          })}
        </div>
        {feedback && (
          <p className={`math-feedback ${isCorrect ? 'positive' : 'gentle'}`} role="status">{feedback}</p>
        )}
        {!profileName && <p className="math-profile-reminder">Dodaj lub wybierz profil, aby zapisywać postępy.</p>}
        <div className="math-question-footer">
          <button className="button button-primary" type="button" onClick={nextQuestion}>
            ✨ Nowe zadanie
          </button>
        </div>
      </div>
      </>
      )}
      {audioError && <p className="math-audio-error" role="status">{audioError}</p>}
      <div className="math-progress-summary" aria-live="polite">
        <span>W tej zabawie: <strong>{currentTopicProgress.correct}</strong> udanych prób</span>
        <span>Wspólne próby: <strong>{progress.attempts}</strong></span>
        <span>Udane próby: <strong>{progress.correct}</strong></span>
      </div>
    </section>
  )
}
