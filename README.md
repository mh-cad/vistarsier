## Disclaimer
This software has not been approved for clinical use and is for research purposes only.

***DUE TO RESOURCING CONSTRAINTS, WE WILL BE UNABLE TO PROVIDE ANY SUPPORT AFTER 29-JUL-2021***

# VisTarsier
VisTarsier is an opensource medical imaging software platform designed to automate multi-study comparisons. 

The first (and currently only) module is a _change detection_ pipeline for MRI brain which takes two 3D FLAIR sequences from two time points and highlights area of increased/decreased signal. It includes coregistration, reslicing, bias-field correction, normalisation, brain surface extration and change detection. New lesions are coloured in orange, whereas area of single reduction are coloured in green. 

The software is series-specifics agnostic; not only will it work on any pair of 3D volumetric FLAIR sequences (in our experience) but it will also successfully compare 3D FLAIR sequences with quite different parameters (e.g. compare a 3D FLAIR sequence with 160 images, to a 3D Fat Saturated FLAIR sequence with 260 images). 

It is designed to perform all of these steps automatically upon recieving appropriate HL7 messages and retrieves studies from PACS and pushes results back to PACS within a few minutes of study completion. 

It has been retrospecitvely (1) and prospectively (2) validated in multiple sclerosis but is not limited to this single indication and can be used in any scenario where followup is performed. 

## Image examples

![Figure 1](https://i.imgur.com/itEQK7r.jpg)

**Figure 1:** VT1: New demyelinating plaque is high-lighted in orange where as an existing lesion that has reduced in size appears as a green doughnut. 


![Figure 2](https://i.imgur.com/8EkHwf6.jpg)

**Figure 2:** VT1: VisTarsier is particularly helpful in patients with high lesion load or where new lesions abut existing ones. 

## Update

The original (validated) version of the software (VT1) has undergone additional development and the current version (VT2) includes a number of improvements: 
1. bias field correction
2. reduction of non-parenchymal signal change
3. improved skull stripping and masking
4. progress and QA summary
5. improved browser-based UI

**Figure 3:** The following images demonstrate VT1 vs VT2 on the same slice of the same patient showing improved lesion visualisation and decreased non-pathological noise. 
![Figure 3](https://i.imgur.com/C8AuExj.jpg)


## Publications

1. [Retrospective validation] Improving Multiple Sclerosis Plaque Detection Using a Semiautomated Assistive Approach. AJNR 2015: https://www.ncbi.nlm.nih.gov/pubmed/26089318 

2. [Prostpecive validation] PACS Integration of Semiautomated Imaging Software Improves Day-to-Day MS Disease Activity Detection AJNR 2019: https://www.ncbi.nlm.nih.gov/pubmed/31515214

3. Neuroradiologists Compared with Non-Neuroradiologists in the Detection of New Multiple Sclerosis Plaques. AJNR 2017: https://www.ncbi.nlm.nih.gov/pubmed/28473341

4. Computer-Aided Detection Can Bridge the Skill Gap in Multiple Sclerosis Monitoring. JACR 2018: https://www.ncbi.nlm.nih.gov/pubmed/28764954

## Grant Support

VisTarsier has been partially funded by a [Royal Melbourne Hosptial](https://www.thermh.org.au/) Foundation Grant. 

## Credits

Associate Professor Frank Gaillard, from the Royal Melbourne Hosptial, has been the lead in this project and has overseen both the development of the app as well as the publications that have stemmed from it to date (2019). A number of developers have been involved over the years, each one enhancing the project. 

- David Rawlinson (now at [Project AGI](https://agi.io/))
- Alan Zhang
- Rajib Chakravorty
- Mehdi Tehrani
- Patrick Prendergast (current)
