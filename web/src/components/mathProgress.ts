export type MathTopic = 'numbers' | 'counting' | 'addition' | 'subtraction' | 'multiplication' | 'division'

export type MathTopicProgress = {
  attempts: number
  correct: number
}

export type MathProgress = {
  attempts: number
  correct: number
  topics: Record<MathTopic, MathTopicProgress>
}

const emptyTopicProgress = (): Record<MathTopic, MathTopicProgress> => ({
  numbers: { attempts: 0, correct: 0 },
  counting: { attempts: 0, correct: 0 },
  addition: { attempts: 0, correct: 0 },
  subtraction: { attempts: 0, correct: 0 },
  multiplication: { attempts: 0, correct: 0 },
  division: { attempts: 0, correct: 0 },
})

export const emptyMathProgress = (): MathProgress => ({
  attempts: 0,
  correct: 0,
  topics: emptyTopicProgress(),
})
