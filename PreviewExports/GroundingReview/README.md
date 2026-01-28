# Native map-object grounding review

Fresh Windows player captures from the tested Animation-01 executable, main runtime SHA-256 `2F65EF341D439949DC5AF99AA6E45243ED81DE6C2B1B0B33385C61770F5FB6B6`. These are actual native 1920x1080 game renders, not concept art or image-generation mockups. The next animation expansion was still being authored when these were captured.

- [Shop overview](Shop/shop-overview.png)
- [Wave 1 environment](Final/depth-1.png)
- [Wave 9 environment](Final/depth-9.png)
- [Wave 17 environment](Final/depth-17.png)

The wave fixtures are physical-shop phases at those depths, not combat-phase screenshots. Their injected diagnostic gold/offer fixtures are not economy measurements.

## Visual findings — not approved as fully grounded

- Blue cabinet/window at upper left and red chair at upper right still read as pasted or suspended against the floor. Their perspective and supporting/contact-shadow cues are insufficient, especially for the chair if it is intended to stand on the floor rather than hang on a wall.
- In wave 7, the small drawer cabinet overlaps the larger blue display; the layering does not clearly communicate physical support.
- Shopkeeper, display cushions, lamp/teacup grouping and continue threshold have stronger contact cues, but their improvement does not establish that all props are fixed.
- Late-stage perimeter architecture makes the composition busier; some overlaps obscure ground contact. This needs a separate art/layout pass, not just a claim that anchor math passes tests.

All native story and shop diagnostic checks completed for these captures. Those checks verify runtime behaviour, not aesthetic approval. No grounding/layout code was changed for this screenshot request.
