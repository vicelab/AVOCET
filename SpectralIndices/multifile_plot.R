######################################################
#                                                    #
# Author: Brian Hungerman                            #
# Date  : 9/24/2018                                  #
# Affil.: VICE Lab, University of California, Merced #
#                                                    #
######################################################

## Necessary Libraries
# If libraries aren't installed on your device, uncomment the following 4 lines
# install.packages("ggplot2")
# install.packages("raster")
# install.packages("tiff")
# install.packages("RStoolbox")
library(ggplot2)
library(raster)
library(tiff)
library(RStoolbox)

##Edit these parameters
folder = "D:\\IMAGES\\AVO_Images\\AVO_PScope4\\WY2016_AVO_PScope4\\images\\use"
file_type = ".tif"
plot_type = "NDVI"

#ANNA: PScope 16-18, NDWI NDVI, clip to extents (to be sent)
# Reclassifying photo at .05 breaks  <-- Ask Josh about this?
# Reclassifying photo at binary (after threshold for wetness) >= 0, < 0   <-- Ask Josh about this?
# Greater than 0, wet, less than 0, dry. Get sum of wet.


# --- At break == 0, not picking up Vernal Pools, picking up stock ponds

## Enters defined Folder
setwd(folder);

## Iterate over every File in Folder with desired File Type
files <- list.files(path=getwd(), pattern=paste("*",file_type, sep=""), full.names=TRUE, recursive=FALSE)
lapply(files, function(file) {
  ## Trim File to just File Name
  # e.g. "D:\IMAGES\AVO_Images\AVO_PScope4\WY2018_AVO_PScope4\images\use\20171002_180525_0f28_3B_AnalyticMS.tif"
  # goes to "20171002_180525_0f28_3B_AnalyticMS"
  file = substr(file, (nchar(getwd())+2), 1000)
  file = substr(file, 0, (nchar(file) - 4))
  
  ## RasterBrick-ize File
  file_brick=brick(paste(file, file_type, sep=""))
  
  ## Calculate Indices
  # Uses Bands 3 (RED) and 4 (NIR)
  output <- spectralIndices(file_brick, blue = paste("X",file, ".1", sep=""), green = paste("X",file, ".2", sep=""),red = paste("X",file, ".3", sep=""), nir = paste("X",file, ".4", sep=""), indices = plot_type)
  
  ## Save Output
  outfile <- writeRaster(output, filename=paste(file,"-",plot_type,file_type, sep=""), format="GTiff", overwrite=TRUE,options=c("INTERLEAVE=BAND","COMPRESS=LZW"))
  
  ## Plot to PDF
  #pdf(paste(file,"-",plot_type,".pdf", sep=""))
  #plot(output)
  #dev.off()
})

##getting bounds for each tiff
##applying a threshold (what is "water"? 1, otherwise "dry" 0)
##combine all tiffs of 







