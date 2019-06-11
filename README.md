# vistarsier
VisTarsier is an opensource medical imaging software platform designed to automate multi-study comparisons. 

The first (and currently only) module is a _change detection_ pipeline for MRI brain which takes two 3D FLAIR sequences from two time points and highlights area of increased/decreased signal. It includes coregistration, reslicing, bias-field correction, normalisation, brain surface extration and change detection. 

It is designed to perform all of these steps automatically upon recieving appropriate HL7 messages and retrieves studies from PACS and pushes results back to PACS within a few minutes of study completion. 

It has been retrospecitvely validated in multiple sclerosis (1) and a prospective study is currently undergoing peer review. 

## Image examples

_pending_

## Publications

1. [retrospecitv validation] Improving Multiple Sclerosis Plaque Detection Using a Semiautomated Assistive Approach. AJNR 2015: https://www.ncbi.nlm.nih.gov/pubmed/26089318

2. Neuroradiologists Compared with Non-Neuroradiologists in the Detection of New Multiple Sclerosis Plaques. AJNR 2017: https://www.ncbi.nlm.nih.gov/pubmed/28473341

3. Computer-Aided Detection Can Bridge the Skill Gap in Multiple Sclerosis Monitoring. JACR 2018: https://www.ncbi.nlm.nih.gov/pubmed/28764954

## Grant Support

VisTarsier has been partially funded by a [Royal Melbourne Hosptial](https://www.thermh.org.au/) Foundation Grant. 

## Credits

Associate Professor Frank Gaillard, from the Royal Melbourne Hosptial, has been the lead in this project and has overseen both the development of the app as well as the publications that have stemmed from it to date (2019). A number of developers have been involved over the years, each one enhancing the project. 

- David Rawlinson (now at [Project AGI](https://agi.io/)
- Alan Zhang
- Rajib Chakravorty
- Mehdi Tehrani
- Patrick Prendergast (current)
