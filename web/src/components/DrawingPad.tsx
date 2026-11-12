import { useEffect, useRef, useState, type PointerEvent } from 'react'

const DRAWING_STROKE_WIDTH = 9
const TRACE_TOLERANCE = DRAWING_STROKE_WIDTH / 2 + 4
const MINIMUM_SKELETON_COVERAGE = 0.9
const MINIMUM_DRAWING_PRECISION = 0.75

function drawLetterGuide(
  canvas: HTMLCanvasElement,
  letter: string,
  variant: 'uppercase' | 'lowercase' = 'uppercase',
) {
  const targetLetter = variant === 'lowercase' ? letter.toLowerCase() : letter.toUpperCase()
  const bounds = canvas.getBoundingClientRect()
  const ratio = Math.min(window.devicePixelRatio || 1, 2)
  canvas.width = Math.round(bounds.width * ratio)
  canvas.height = Math.round(bounds.height * ratio)
  const context = canvas.getContext('2d')
  const targetCanvas = document.createElement('canvas')
  targetCanvas.width = canvas.width
  targetCanvas.height = canvas.height
  const targetContext = targetCanvas.getContext('2d')
  if (!context || !targetContext) return null
  context.setTransform(ratio, 0, 0, ratio, 0, 0)
  targetContext.setTransform(ratio, 0, 0, ratio, 0, 0)
  const fontSize = Math.min(bounds.height * 0.8, bounds.width * 0.72, 220)
  const font = `700 ${fontSize}px "Nunito", sans-serif`
  context.textAlign = 'center'
  context.textBaseline = 'middle'
  context.fillStyle = '#c4d1bf'
  context.font = font
  context.fillText(targetLetter, bounds.width / 2, bounds.height / 2)
  targetContext.textAlign = 'center'
  targetContext.textBaseline = 'middle'
  targetContext.font = font
  targetContext.fillStyle = '#fff'
  targetContext.fillText(targetLetter, bounds.width / 2, bounds.height / 2)
  return targetCanvas
}

function thinMask(mask: Uint8Array, width: number, height: number) {
  const neighbors = new Uint8Array(8)
  let changed = true

  while (changed) {
    changed = false
    for (let phase = 0; phase < 2; phase++) {
      const pixelsToRemove: number[] = []
      for (let y = 1; y < height - 1; y++) {
        for (let x = 1; x < width - 1; x++) {
          const index = y * width + x
          if (mask[index] === 0) continue

          neighbors[0] = mask[index - width]
          neighbors[1] = mask[index - width + 1]
          neighbors[2] = mask[index + 1]
          neighbors[3] = mask[index + width + 1]
          neighbors[4] = mask[index + width]
          neighbors[5] = mask[index + width - 1]
          neighbors[6] = mask[index - 1]
          neighbors[7] = mask[index - width - 1]

          let neighborCount = 0
          let transitions = 0
          for (let neighbor = 0; neighbor < neighbors.length; neighbor++) {
            neighborCount += neighbors[neighbor]
            if (neighbors[neighbor] === 0 && neighbors[(neighbor + 1) % neighbors.length] === 1) {
              transitions++
            }
          }

          if (neighborCount < 2 || neighborCount > 6 || transitions !== 1) continue

          const north = neighbors[0]
          const east = neighbors[2]
          const south = neighbors[4]
          const west = neighbors[6]
          const preservesShape = phase === 0
            ? north * east * south === 0 && east * south * west === 0
            : north * east * west === 0 && north * south * west === 0
          if (preservesShape) pixelsToRemove.push(index)
        }
      }

      for (const index of pixelsToRemove) {
        mask[index] = 0
      }
      changed ||= pixelsToRemove.length > 0
    }
  }

  return mask
}

function isTraceOnTarget(
  inkCanvas: HTMLCanvasElement,
  targetCanvas: HTMLCanvasElement,
  displayWidth: number,
) {
  const inkContext = inkCanvas.getContext('2d')
  const targetContext = targetCanvas.getContext('2d')
  if (!inkContext || !targetContext) return false

  const scale = inkCanvas.width / Math.max(displayWidth, 1)
  const width = Math.max(1, Math.round(inkCanvas.width / scale))
  const height = Math.max(1, Math.round(inkCanvas.height / scale))
  const ink = inkContext.getImageData(0, 0, inkCanvas.width, inkCanvas.height).data
  const target = targetContext.getImageData(0, 0, targetCanvas.width, targetCanvas.height).data
  const inkMask = new Uint8Array(width * height)
  const targetMask = new Uint8Array(width * height)
  const inkBounds = {
    minX: width,
    minY: height,
    maxX: -1,
    maxY: -1,
  }
  const targetBounds = {
    minX: width,
    minY: height,
    maxX: -1,
    maxY: -1,
  }

  for (let y = 0; y < height; y++) {
    const sourceY = Math.min(Math.floor((y + 0.5) * scale), inkCanvas.height - 1)
    for (let x = 0; x < width; x++) {
      const sourceX = Math.min(Math.floor((x + 0.5) * scale), inkCanvas.width - 1)
      const sourceIndex = (sourceY * inkCanvas.width + sourceX) * 4 + 3
      const index = y * width + x

      if (ink[sourceIndex] >= 70) {
        inkMask[index] = 1
        inkBounds.minX = Math.min(inkBounds.minX, x)
        inkBounds.minY = Math.min(inkBounds.minY, y)
        inkBounds.maxX = Math.max(inkBounds.maxX, x)
        inkBounds.maxY = Math.max(inkBounds.maxY, y)
      }
      if (target[sourceIndex] >= 70) {
        targetMask[index] = 1
        targetBounds.minX = Math.min(targetBounds.minX, x)
        targetBounds.minY = Math.min(targetBounds.minY, y)
        targetBounds.maxX = Math.max(targetBounds.maxX, x)
        targetBounds.maxY = Math.max(targetBounds.maxY, y)
      }
    }
  }

  if (inkBounds.maxX < 0 || targetBounds.maxX < 0) return false

  const skeleton = thinMask(targetMask.slice(), width, height)
  const scaleRadius = Math.ceil(TRACE_TOLERANCE * scale)
  const allowedMask = new Uint8Array(width * height)
  let skeletonPixels = 0
  let coveredSkeletonPixels = 0
  let drawnPixels = 0
  let allowedDrawingPixels = 0

  for (let y = targetBounds.minY; y <= targetBounds.maxY; y++) {
    for (let x = targetBounds.minX; x <= targetBounds.maxX; x++) {
      const index = y * width + x
      if (skeleton[index] === 0) continue

      skeletonPixels++
      let isCovered = false
      for (let offsetY = -scaleRadius; offsetY <= scaleRadius; offsetY++) {
        for (let offsetX = -scaleRadius; offsetX <= scaleRadius; offsetX++) {
          if (offsetX * offsetX + offsetY * offsetY > scaleRadius * scaleRadius) continue
          const nearbyX = x + offsetX
          const nearbyY = y + offsetY
          if (
            nearbyX >= 0 && nearbyX < width &&
            nearbyY >= 0 && nearbyY < height
          ) {
            allowedMask[nearbyY * width + nearbyX] = 1
            if (inkMask[nearbyY * width + nearbyX] === 1) {
              isCovered = true
            }
          }
        }
      }
      if (isCovered) coveredSkeletonPixels++
    }
  }

  for (let index = 0; index < inkMask.length; index++) {
    if (inkMask[index] === 0) continue
    drawnPixels++
    if (allowedMask[index] === 1) allowedDrawingPixels++
  }

  if (drawnPixels < DRAWING_STROKE_WIDTH * 10 || skeletonPixels === 0) return false

  const skeletonCoverage = coveredSkeletonPixels / skeletonPixels
  const drawingPrecision = allowedDrawingPixels / drawnPixels
  return skeletonCoverage >= MINIMUM_SKELETON_COVERAGE &&
    drawingPrecision >= MINIMUM_DRAWING_PRECISION
}

export function DrawingPad({
  letter,
  variant = 'uppercase',
  onComplete,
  large = false,
  subjectLabel = 'literę',
}: {
  letter: string
  variant?: 'uppercase' | 'lowercase'
  onComplete: (isOnTarget: boolean) => void
  large?: boolean
  subjectLabel?: string
}) {
  const canvasRef = useRef<HTMLCanvasElement>(null)
  const inkCanvasRef = useRef<HTMLCanvasElement>(document.createElement('canvas'))
  const targetCanvasRef = useRef<HTMLCanvasElement | null>(null)
  const drawingRef = useRef(false)
  const hasDrawnRef = useRef(false)
  const attemptFinishedRef = useRef(false)
  const boundsRef = useRef<DOMRect | null>(null)
  const lastPointRef = useRef<{ x: number; y: number } | null>(null)
  const paintedPointRef = useRef<{ x: number; y: number } | null>(null)
  const pendingPointsRef = useRef<{ x: number; y: number }[]>([])
  const frameRef = useRef<number | null>(null)
  const onCompleteRef = useRef(onComplete)
  const [traceMessage, setTraceMessage] = useState('')

  useEffect(() => {
    onCompleteRef.current = onComplete
  }, [onComplete])

  useEffect(() => {
    const canvas = canvasRef.current
    if (!canvas) return

    const resetCanvases = () => {
      targetCanvasRef.current = drawLetterGuide(canvas, letter, variant)
      inkCanvasRef.current.width = canvas.width
      inkCanvasRef.current.height = canvas.height
      const inkContext = inkCanvasRef.current.getContext('2d')
      const ratio = canvas.width / Math.max(canvas.clientWidth, 1)
      inkContext?.setTransform(ratio, 0, 0, ratio, 0, 0)
      attemptFinishedRef.current = false
      setTraceMessage('')
    }

    resetCanvases()
    const observer = new ResizeObserver(resetCanvases)
    observer.observe(canvas)
    return () => {
      observer.disconnect()
    }
  }, [letter, large, variant])

  function paintPendingPoints() {
    frameRef.current = null
    const canvas = canvasRef.current
    const context = canvas?.getContext('2d')
    const inkContext = inkCanvasRef.current.getContext('2d')
    const points = pendingPointsRef.current
    const lastPoint = paintedPointRef.current
    if (!context || !inkContext || !lastPoint || points.length === 0) return

    for (const drawingContext of [context, inkContext]) {
      drawingContext.beginPath()
      drawingContext.moveTo(lastPoint.x, lastPoint.y)
      drawingContext.strokeStyle = '#4f8658'
      drawingContext.lineWidth = DRAWING_STROKE_WIDTH
      drawingContext.lineCap = 'round'
      drawingContext.lineJoin = 'round'
      for (const point of points) {
        drawingContext.lineTo(point.x, point.y)
      }
      drawingContext.stroke()
    }

    for (const point of points) {
      paintedPointRef.current = point
    }
    pendingPointsRef.current = []
    hasDrawnRef.current = true
  }

  function draw(event: PointerEvent<HTMLCanvasElement>) {
    if (!drawingRef.current) return
    const bounds = boundsRef.current
    if (!bounds) return
    const coalescedEvents = event.nativeEvent.getCoalescedEvents?.()
    const events = coalescedEvents?.length ? coalescedEvents : [event.nativeEvent]

    for (const eventPoint of events) {
      const point = { x: eventPoint.clientX - bounds.left, y: eventPoint.clientY - bounds.top }
      const previousPoint = lastPointRef.current
      if (previousPoint) {
        const distance = Math.hypot(point.x - previousPoint.x, point.y - previousPoint.y)
        const travelled = Math.ceil(distance / 4)
        for (let step = 1; step <= travelled; step++) {
          const ratio = step / travelled
          pendingPointsRef.current.push({
            x: previousPoint.x + (point.x - previousPoint.x) * ratio,
            y: previousPoint.y + (point.y - previousPoint.y) * ratio,
          })
        }
      }
      lastPointRef.current = point
    }

    if (frameRef.current === null) {
      frameRef.current = requestAnimationFrame(paintPendingPoints)
    }
  }

  function startDrawing(event: PointerEvent<HTMLCanvasElement>) {
    const canvas = canvasRef.current
    if (!canvas || !canvas.getContext('2d')) return
    if (attemptFinishedRef.current) {
      clearDrawing()
      attemptFinishedRef.current = false
    }
    canvas.setPointerCapture(event.pointerId)
    boundsRef.current = canvas.getBoundingClientRect()
    drawingRef.current = true
    const point = {
      x: event.clientX - boundsRef.current.left,
      y: event.clientY - boundsRef.current.top,
    }
    lastPointRef.current = point
    paintedPointRef.current = point
    pendingPointsRef.current = []
    setTraceMessage('')
  }

  function stopDrawing() {
    if (frameRef.current !== null) {
      cancelAnimationFrame(frameRef.current)
      frameRef.current = null
    }
    paintPendingPoints()
    drawingRef.current = false
    boundsRef.current = null
    lastPointRef.current = null
    if (hasDrawnRef.current) {
      hasDrawnRef.current = false
      const isOnTarget = targetCanvasRef.current && canvasRef.current
        ? isTraceOnTarget(
            inkCanvasRef.current,
            targetCanvasRef.current,
            canvasRef.current.clientWidth,
          )
        : false
      setTraceMessage(
        isOnTarget
            ? `Świetnie! Narysowałeś cały kształt ${subjectLabel}. 🌟`
            : `Spróbuj jeszcze raz i prowadź palec bliżej kształtu ${subjectLabel}.`,
      )
      onCompleteRef.current(isOnTarget)
      if (isOnTarget) {
        attemptFinishedRef.current = true
        window.setTimeout(() => {
          clearDrawing()
        }, 260)
      }
    }
    paintedPointRef.current = null
  }

  function clearDrawing() {
    const canvas = canvasRef.current
    if (!canvas) return
    pendingPointsRef.current = []
    lastPointRef.current = null
    paintedPointRef.current = null
    hasDrawnRef.current = false
    attemptFinishedRef.current = false
    targetCanvasRef.current = drawLetterGuide(canvas, letter, variant)
    inkCanvasRef.current.width = canvas.width
    inkCanvasRef.current.height = canvas.height
    const inkContext = inkCanvasRef.current.getContext('2d')
    const ratio = canvas.width / Math.max(canvas.clientWidth, 1)
    inkContext?.setTransform(ratio, 0, 0, ratio, 0, 0)
    setTraceMessage('')
  }

  return (
    <div className={large ? 'drawing-wrap drawing-wrap-large' : 'drawing-wrap'}>
      <canvas
        ref={canvasRef}
        className={large ? 'drawing-canvas drawing-canvas-large' : 'drawing-canvas'}
        aria-label={`Pole do rysowania ${subjectLabel} ${variant === 'lowercase' ? letter.toLowerCase() : letter.toUpperCase()}`}
        onPointerDown={startDrawing}
        onPointerMove={draw}
        onPointerUp={stopDrawing}
        onPointerCancel={stopDrawing}
      />
      <button className="text-button" type="button" onClick={clearDrawing}>
        Wyczyść rysunek
      </button>
      {traceMessage && (
        <p
          className={traceMessage.startsWith('Świetnie') ? 'trace-message trace-success' : 'trace-message trace-retry'}
          role="status"
        >
          {traceMessage}
        </p>
      )}
    </div>
  )
}
