import React from 'react'
import useEmblaCarousel from 'embla-carousel-react'
import './Carousel.css'

export function EmblaCarousel({ slides = [], options = {} }) {
    const [emblaRef, emblaApi] = useEmblaCarousel({ loop: true, ...options })

    const goToPrev = () => emblaApi?.scrollPrev()
    const goToNext = () => emblaApi?.scrollNext()

    return (
        <div className="embla">
            <div className="embla__viewport" ref={emblaRef}>
                <div className="embla__container">
                    {slides.map((src, index) => (
                        <div className="embla__slide" key={index}>
                            <img src={src} alt={`Zone image ${index + 1}`} className="embla__slide__img" />
                        </div>
                    ))}
                </div>
            </div>

            <button className="embla__prev" onClick={goToPrev}>
                ◀
            </button>
            <button className="embla__next" onClick={goToNext}>
                ▶
            </button>
        </div>
    )
}