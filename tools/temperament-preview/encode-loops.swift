import Foundation
import ImageIO
import UniformTypeIdentifiers
guard CommandLine.arguments.count == 2 else { fatalError("Supply output folder") }
let folder=URL(fileURLWithPath:CommandLine.arguments[1])
let root=folder.appendingPathComponent("frames")
let output=folder.appendingPathComponent("loops")
try FileManager.default.createDirectory(at:output,withIntermediateDirectories:true)
let actions=["sunny","grumpy","shy","show-off","dreamy","sleepy","mischief","calm"]
func gif(_ frames:[URL],_ file:URL) {
 guard let dest=CGImageDestinationCreateWithURL(file as CFURL,UTType.gif.identifier as CFString,frames.count,nil) else { fatalError("GIF destination unavailable") }
 CGImageDestinationSetProperties(dest,[kCGImagePropertyGIFDictionary:[kCGImagePropertyGIFLoopCount:0]] as CFDictionary)
 for frame in frames {
  guard let source=CGImageSourceCreateWithURL(frame as CFURL,nil),let image=CGImageSourceCreateImageAtIndex(source,0,nil) else { fatalError("Missing rendered frame") }
  CGImageDestinationAddImage(dest,image,[kCGImagePropertyGIFDictionary:[kCGImagePropertyGIFDelayTime:0.07]] as CFDictionary)
 }
 if !CGImageDestinationFinalize(dest) { fatalError("GIF write failed") }
}

for action in actions {
 let frames=(0..<64).map { root.appendingPathComponent(action).appendingPathComponent(String(format:"%03d.png",$0)) }
 gif(frames,output.appendingPathComponent(action+".gif"))
}

print("Created eight Pip face loops.")
